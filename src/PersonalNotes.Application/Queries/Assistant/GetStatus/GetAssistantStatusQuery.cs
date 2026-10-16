using MediatR;

namespace PersonalNotes.Application.Queries.Assistant.GetStatus
{
    public record GetAssistantStatusQuery : IRequest<GetAssistantStatusResponse>;

    /// <param name="Configured">true si el servidor tiene Gemini:ApiKey (la clave nunca se devuelve).</param>
    public record GetAssistantStatusResponse(bool Configured, string Provider, string Model);
}
