using PersonalNotes.Application.DTOs;
using PersonalNotes.Application.Services.Assistant;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Tests.Application
{
    public class AssistantPromptBuilderTests
    {
        private readonly AssistantPromptBuilder _builder = new();

        [Fact]
        public void BuildPrompt_WithoutContext_IsTheTrimmedQuestion()
        {
            Assert.Equal("¿Qué es CQRS?", _builder.BuildPrompt("  ¿Qué es CQRS? ", null));
        }

        [Fact]
        public void BuildPrompt_AttachesDocumentAsContext()
        {
            var prompt = _builder.BuildPrompt("Resume en 3 puntos.", new DocumentContextDto("Sprint 28", "Entregables clave"));

            Assert.Equal("Documento \"Sprint 28\":\n\"\"\"\nEntregables clave\n\"\"\"\n\nResume en 3 puntos.", prompt);
        }

        [Fact]
        public void BuildPrompt_UsesDefaultsForEmptyTitleAndContent_AndTruncatesLongContext()
        {
            var vacio = _builder.BuildPrompt("Hola", new DocumentContextDto(" ", ""));
            Assert.StartsWith("Documento \"Sin título\":\n\"\"\"\n(sin contenido)", vacio);

            var largo = _builder.BuildPrompt("Hola", new DocumentContextDto("Nota", new string('a', AssistantPromptBuilder.MaxContextLength + 100)));
            Assert.Contains(new string('a', AssistantPromptBuilder.MaxContextLength) + "\n", largo);
            Assert.DoesNotContain(new string('a', AssistantPromptBuilder.MaxContextLength + 1), largo);
        }

        [Fact]
        public void BuildHistory_KeepsLastMessagesInOrder_AndSkipsEmptyOnes()
        {
            var history = Enumerable.Range(1, 15)
                .Select(i => new ChatMessageDto(i % 2 == 1 ? ChatRole.User : ChatRole.Assistant, $"m{i}"))
                .Append(new ChatMessageDto(ChatRole.User, "  "))
                .ToList();

            var turns = _builder.BuildHistory(history);

            Assert.Equal(AssistantPromptBuilder.MaxHistoryMessages, turns.Count);
            Assert.Equal("m6", turns[0].Text);
            Assert.Equal(("m15", ChatRole.User), (turns[^1].Text, turns[^1].Role));
            Assert.Empty(_builder.BuildHistory(null));
        }
    }
}
