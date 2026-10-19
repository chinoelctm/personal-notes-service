using PersonalNotes.Application.DTOs;

namespace PersonalNotes.Api.Models.Assistant.Ask
{
    /// <param name="Question">Pregunta del usuario.</param>
    /// <param name="Context">Documento activo como contexto { title, text, documentId? } (texto plano); null para una pregunta general.</param>
    /// <param name="History">Mensajes previos { role: User|Assistant, text }; se usan los últimos 10.</param>
    /// <param name="SearchNotes">true (por defecto): la IA recibe el catálogo y el contenido relevante de todas las notas.</param>
    public record AskAssistantRequest(
        string Question,
        DocumentContextDto? Context,
        List<ChatMessageDto>? History,
        bool? SearchNotes);
}
