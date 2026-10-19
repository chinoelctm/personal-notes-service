using System.Text;
using PersonalNotes.Application.DTOs;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Services.Assistant
{
    /// <summary>
    /// Arma lo que se envía al proveedor de IA: la pregunta con las notas del usuario (catálogo y contenido
    /// relevante), el documento abierto como contexto y el historial recortado a los últimos mensajes
    /// (para no exceder el límite gratuito de tokens).
    /// </summary>
    public class AssistantPromptBuilder
    {
        public const int MaxQuestionLength = 4_000;
        public const int MaxContextLength = 30_000;
        public const int MaxHistoryMessages = 10;
        public const int MaxHistoryMessageLength = 8_000;

        public string BuildPrompt(string question, DocumentContextDto? context, NotesKnowledge? knowledge = null)
        {
            var pregunta = question.Trim();
            if (context is null && knowledge is null) return pregunta;

            var prompt = new StringBuilder();

            if (knowledge is not null)
            {
                prompt.Append($"Notas del usuario ({knowledge.TotalNotes} documento(s) en total):\n");
                prompt.Append(knowledge.TotalNotes == 0 ? "(el usuario todavía no tiene documentos)" : knowledge.Catalog);
                prompt.Append("\n\n");

                if (knowledge.Excerpts.Length > 0)
                {
                    prompt.Append("Contenido de las notas más relevantes para la pregunta:\n");
                    prompt.Append(knowledge.Excerpts);
                    prompt.Append("\n\n");
                }
            }

            if (context is not null)
            {
                var titulo = string.IsNullOrWhiteSpace(context.Title) ? "Sin título" : context.Title.Trim();
                var texto = (context.Text ?? string.Empty).Trim();
                if (texto.Length > MaxContextLength) texto = texto[..MaxContextLength];
                if (texto.Length == 0) texto = "(sin contenido)";

                prompt.Append($"Documento abierto \"{titulo}\":\n\"\"\"\n{texto}\n\"\"\"\n\n");
            }

            if (knowledge is not null)
            {
                prompt.Append("Responde basándote en estas notas y menciona el título del documento de donde sacas cada dato. ");
                prompt.Append("Si la respuesta no está en las notas, dilo claramente antes de usar conocimiento general.\n\n");
            }

            prompt.Append(context is null && knowledge is null ? pregunta : $"Pregunta: {pregunta}");
            return prompt.ToString();
        }

        /// <summary>Últimos mensajes no vacíos, en orden, cada uno recortado.</summary>
        public IReadOnlyList<ChatTurn> BuildHistory(IEnumerable<ChatMessageDto>? history)
        {
            return (history ?? [])
                .Where(m => !string.IsNullOrWhiteSpace(m.Text))
                .TakeLast(MaxHistoryMessages)
                .Select(m => new ChatTurn(m.Role,
                    m.Text.Length > MaxHistoryMessageLength ? m.Text[..MaxHistoryMessageLength] : m.Text))
                .ToList();
        }
    }
}
