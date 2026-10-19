using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Assistant;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Tests.Application
{
    public class NotesKnowledgeBuilderTests
    {
        private static readonly DateTime Now = new(2026, 10, 19, 12, 0, 0, DateTimeKind.Utc);
        private readonly NotesKnowledgeBuilder _builder = new();

        private static NoteForAssistant Note(string id, string title, string text, int hoursAgo = 1, string path = "Pruebas IA") =>
            new(id, title, path, text, Priority.None, Now.AddHours(-hoursAgo));

        private readonly List<NoteForAssistant> _notes =
        [
            Note("arriendo", "Contrato de arriendo depto Ñuñoa", "Monto mensual: 650.000 CLP. Vence el 31 de marzo de 2027.", hoursAgo: 5),
            Note("pan", "Receta de pan de masa madre", "500 g de harina, 350 ml de agua. Horno a 250 °C.", hoursAgo: 3),
            Note("staging", "Accesos del servidor de staging", "Puerto 8443. Despliegue los jueves. Responsable: Camila Rojas.", hoursAgo: 1)
        ];

        [Fact]
        public void Build_PicksTheRelevantNote_IgnoringAccentsAndStopWords()
        {
            var knowledge = _builder.Build("¿Cuánto pago de ARRIENDO en Nunoa y cuándo vence?", _notes);

            Assert.Equal("arriendo", knowledge.Sources[0].Id);
            Assert.DoesNotContain(knowledge.Sources, s => s.Id == "pan");
            Assert.Contains("650.000 CLP", knowledge.Excerpts);
            Assert.Equal(3, knowledge.TotalNotes);
        }

        [Fact]
        public void Build_MatchesContentNotOnlyTitles()
        {
            var knowledge = _builder.Build("¿Quién es responsable del despliegue y qué puerto usa?", _notes);

            Assert.Equal(["staging"], knowledge.Sources.Select(s => s.Id));
        }

        [Fact]
        public void Build_WithoutMatches_SendsTheMostRecentNotes()
        {
            var knowledge = _builder.Build("Resume lo último", _notes);

            Assert.Equal(["staging", "pan", "arriendo"], knowledge.Sources.Select(s => s.Id));
        }

        [Fact]
        public void Build_CatalogListsEveryNoteWithFolderAndDate()
        {
            var knowledge = _builder.Build("¿Qué tengo?", _notes);

            Assert.Equal(3, knowledge.Catalog.Split('\n').Length);
            Assert.Contains("- Receta de pan de masa madre (carpeta: Pruebas IA; prioridad: ninguna; editado: 2026-10-19)", knowledge.Catalog);
        }

        [Fact]
        public void Build_ExcludesTheOpenDocument()
        {
            var knowledge = _builder.Build("arriendo", _notes, excludeId: "arriendo");

            Assert.DoesNotContain(knowledge.Sources, s => s.Id == "arriendo");
        }

        [Fact]
        public void Build_RespectsTheContentBudget_ButAlwaysSendsAtLeastOneNote()
        {
            var huge = new string('x', NotesKnowledgeBuilder.MaxNoteLength * 2);
            var notes = Enumerable.Range(1, 10).Select(i => Note($"n{i}", $"Bitácora {i}", $"bitacora {huge}")).ToList();

            var knowledge = _builder.Build("bitácora", notes);

            Assert.True(knowledge.Excerpts.Length <= NotesKnowledgeBuilder.MaxExcerptsLength);
            Assert.InRange(knowledge.Sources.Count, 1, 9);
        }

        [Fact]
        public void HtmlText_RemovesTagsAndBase64Images_AndDecodesEntities()
        {
            var html = "<h2>Entregables</h2><p>Revisión &amp; pruebas</p><img src=\"data:image/png;base64,AAAA\"><ul><li>Uno</li><li>Dos</li></ul>";

            Assert.Equal("Entregables Revisión & pruebas Uno Dos", HtmlText.ToPlainText(html));
            Assert.Equal(string.Empty, HtmlText.ToPlainText(null));
        }
    }
}
