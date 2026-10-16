using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.DTOs
{
    /// <param name="Role">User o Assistant.</param>
    public record ChatMessageDto(ChatRole Role, string Text);

    /// <summary>Documento activo adjunto como contexto (texto plano, puede ser un borrador sin publicar).</summary>
    public record DocumentContextDto(string Title, string Text);
}
