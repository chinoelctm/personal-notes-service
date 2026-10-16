using MediatR;
using PersonalNotes.Application.DTOs;

namespace PersonalNotes.Application.Commands.Assistant.Ask
{
    /// <param name="Context">Documento adjunto ("Preguntar sobre este documento"); null para una pregunta general.</param>
    /// <param name="History">Mensajes previos de la conversación (se usan los últimos).</param>
    public record AskAssistantCommand(string Question, DocumentContextDto? Context, List<ChatMessageDto>? History)
        : IRequest<AskAssistantResponse>;

    public record AskAssistantResponse(string Answer, string Model);
}
