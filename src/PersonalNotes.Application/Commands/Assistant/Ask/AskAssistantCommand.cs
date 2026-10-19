using MediatR;
using PersonalNotes.Application.DTOs;

namespace PersonalNotes.Application.Commands.Assistant.Ask
{
    /// <param name="Context">Documento adjunto ("Preguntar sobre este documento"); null para una pregunta general.</param>
    /// <param name="History">Mensajes previos de la conversación (se usan los últimos).</param>
    /// <param name="SearchNotes">true o null: la IA también recibe el catálogo y el contenido relevante de todas las notas.</param>
    public record AskAssistantCommand(string Question, DocumentContextDto? Context, List<ChatMessageDto>? History,
        bool? SearchNotes = null) : IRequest<AskAssistantResponse>;

    /// <param name="Sources">Documentos cuyo contenido se envió a la IA (vacío si no se buscó en las notas).</param>
    public record AskAssistantResponse(string Answer, string Model, List<AssistantSourceDto> Sources);
}
