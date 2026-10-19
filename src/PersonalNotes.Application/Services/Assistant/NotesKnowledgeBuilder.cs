using System.Globalization;
using System.Text;
using PersonalNotes.Application.DTOs;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Services.Assistant
{
    /// <summary>Nota del usuario lista para la IA: texto plano y ruta de su carpeta.</summary>
    public record NoteForAssistant(string Id, string Title, string Path, string Text, Priority Priority, DateTime UpdatedOn);

    /// <param name="Catalog">Una línea por documento (título, carpeta, prioridad, fecha): permite responder "qué notas tengo".</param>
    /// <param name="Excerpts">Contenido de los documentos más relevantes para la pregunta.</param>
    /// <param name="Sources">Documentos que coinciden con la pregunta y cuyo contenido se envió (el front los muestra como fuentes).
    /// Vacío si ninguno coincidió: las notas recientes se envían igual como contexto, pero no son fuentes.</param>
    public record NotesKnowledge(string Catalog, string Excerpts, int TotalNotes, List<AssistantSourceDto> Sources);

    /// <summary>
    /// Arma la "base de conocimiento" que acompaña cada pregunta: el catálogo de todas las notas y el contenido
    /// de las más relevantes, elegidas por coincidencia de palabras (sin tildes ni mayúsculas) dentro de un
    /// presupuesto de caracteres para no exceder el límite gratuito de tokens.
    /// </summary>
    public class NotesKnowledgeBuilder
    {
        public const int MaxCatalogNotes = 200;
        public const int MaxExcerptsLength = 40_000;
        public const int MaxNoteLength = 8_000;
        public const int MaxNotesWithoutMatch = 5;

        private const int TitleWeight = 5;
        private const int PathWeight = 2;
        private const int MaxOccurrencesPerTerm = 10;

        // Palabras frecuentes que no ayudan a encontrar el documento
        private static readonly HashSet<string> StopWords =
        [
            "que", "cual", "cuales", "como", "cuando", "donde", "quien", "cuanto", "cuanta", "cuantos", "cuantas",
            "para", "por", "con", "sin", "sobre", "entre", "desde", "hasta", "del", "los", "las", "una", "uno", "unos",
            "unas", "este", "esta", "estos", "estas", "ese", "esa", "eso", "esto", "mis", "tus", "sus", "nuestro",
            "tengo", "tiene", "tienen", "hay", "son", "fue", "ser", "esta", "estan", "dime", "dame", "puedes",
            "sabes", "nota", "notas", "documento", "documentos", "mas", "muy", "pero", "and", "the"
        ];

        public NotesKnowledge Build(string question, IReadOnlyCollection<NoteForAssistant> notes, string? excludeId = null)
        {
            var catalog = BuildCatalog(notes);
            var candidates = notes.Where(n => n.Id != excludeId).ToList();
            var terms = Terms(question);

            var ranked = candidates
                .Select(n => (Note: n, Score: Score(n, terms)))
                .Where(x => x.Score > 0)
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Note.UpdatedOn)
                .Select(x => x.Note)
                .ToList();

            // Sin coincidencias ("resume lo último que escribí"): los editados más recientemente
            var matched = ranked.Count > 0;
            if (!matched)
                ranked = candidates.OrderByDescending(n => n.UpdatedOn).Take(MaxNotesWithoutMatch).ToList();

            var excerpts = new StringBuilder();
            var sources = new List<AssistantSourceDto>();
            var included = 0;
            foreach (var note in ranked)
            {
                var text = note.Text.Length > MaxNoteLength ? note.Text[..MaxNoteLength] + "…" : note.Text;
                var block = $"### {note.Title} (carpeta: {note.Path})\n{(text.Length == 0 ? "(sin contenido)" : text)}\n\n";
                if (excerpts.Length + block.Length > MaxExcerptsLength && included > 0) break;

                excerpts.Append(block);
                if (matched) sources.Add(new AssistantSourceDto(note.Id, note.Title, note.Path));
                included++;
            }

            return new NotesKnowledge(catalog, excerpts.ToString().TrimEnd(), notes.Count, sources);
        }

        /// <summary>Minúsculas y sin tildes: "Ñuñoa" coincide con "nunoa".</summary>
        public static string Normalize(string text)
        {
            var decomposed = text.ToLowerInvariant().Normalize(NormalizationForm.FormD);
            var builder = new StringBuilder(decomposed.Length);
            foreach (var c in decomposed)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    builder.Append(c);
            }
            return builder.ToString().Normalize(NormalizationForm.FormC);
        }

        private static IReadOnlyList<string> Terms(string question)
        {
            return Normalize(question)
                .Split((char[])[' ', '\t', '\n', '\r', ',', '.', ';', ':', '?', '¿', '!', '¡', '"', '\'', '(', ')', '/', '-'],
                    StringSplitOptions.RemoveEmptyEntries)
                .Where(t => t.Length >= 3 && !StopWords.Contains(t))
                .Distinct()
                .ToList();
        }

        private static int Score(NoteForAssistant note, IReadOnlyList<string> terms)
        {
            if (terms.Count == 0) return 0;

            var title = Normalize(note.Title);
            var path = Normalize(note.Path);
            var text = Normalize(note.Text);

            var score = 0;
            foreach (var term in terms)
            {
                if (title.Contains(term)) score += TitleWeight;
                if (path.Contains(term)) score += PathWeight;
                score += Math.Min(Occurrences(text, term), MaxOccurrencesPerTerm);
            }
            return score;
        }

        private static int Occurrences(string text, string term)
        {
            var count = 0;
            for (var i = text.IndexOf(term, StringComparison.Ordinal); i >= 0; i = text.IndexOf(term, i + term.Length, StringComparison.Ordinal))
                count++;
            return count;
        }

        private static string BuildCatalog(IReadOnlyCollection<NoteForAssistant> notes)
        {
            var lines = notes
                .OrderByDescending(n => n.UpdatedOn)
                .Take(MaxCatalogNotes)
                .Select(n => $"- {n.Title} (carpeta: {n.Path}; prioridad: {PriorityName(n.Priority)}; editado: {n.UpdatedOn:yyyy-MM-dd})");

            var catalog = string.Join("\n", lines);
            return notes.Count > MaxCatalogNotes
                ? $"{catalog}\n- … y {notes.Count - MaxCatalogNotes} documentos más"
                : catalog;
        }

        private static string PriorityName(Priority priority) => priority switch
        {
            Priority.High => "alta",
            Priority.Medium => "media",
            Priority.Low => "baja",
            _ => "ninguna"
        };
    }
}
