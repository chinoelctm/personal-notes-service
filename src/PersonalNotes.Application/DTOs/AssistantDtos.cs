using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.DTOs
{
    /// <param name="Role">User o Assistant.</param>
    public record ChatMessageDto(ChatRole Role, string Text);

    /// <summary>Documento activo adjunto como contexto (texto plano, puede ser un borrador sin publicar).</summary>
    /// <param name="DocumentId">Id del documento si ya está publicado (no se repite al buscar en las notas).</param>
    public record DocumentContextDto(string Title, string Text, string? DocumentId = null);

    /// <summary>Documento cuyo contenido se envió a la IA para responder.</summary>
    public record AssistantSourceDto(string Id, string Title, string Path);
}
