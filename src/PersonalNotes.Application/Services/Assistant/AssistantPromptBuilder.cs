using PersonalNotes.Application.DTOs;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Services.Assistant
{
    /// <summary>
    /// Arma lo que se envía al proveedor de IA: la pregunta con el documento adjunto como contexto y el
    /// historial recortado a los últimos mensajes (para no exceder el límite gratuito de tokens).
    /// </summary>
    public class AssistantPromptBuilder
    {
        public const int MaxQuestionLength = 4_000;
        public const int MaxContextLength = 30_000;
        public const int MaxHistoryMessages = 10;
        public const int MaxHistoryMessageLength = 8_000;

        public string BuildPrompt(string question, DocumentContextDto? context)
        {
            var pregunta = question.Trim();
            if (context is null) return pregunta;

            var titulo = string.IsNullOrWhiteSpace(context.Title) ? "Sin título" : context.Title.Trim();
            var texto = (context.Text ?? string.Empty).Trim();
            if (texto.Length > MaxContextLength) texto = texto[..MaxContextLength];
            if (texto.Length == 0) texto = "(sin contenido)";

            return $"Documento \"{titulo}\":\n\"\"\"\n{texto}\n\"\"\"\n\n{pregunta}";
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
