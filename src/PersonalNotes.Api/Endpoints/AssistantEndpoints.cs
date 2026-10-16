using Mapster;
using MediatR;
using PersonalNotes.Api.Models.Assistant.Ask;
using PersonalNotes.Application.Commands.Assistant.Ask;
using PersonalNotes.Application.Queries.Assistant.GetStatus;

namespace PersonalNotes.Api.Endpoints
{
    public static class AssistantEndpoints
    {
        public static IEndpointRouteBuilder MapAssistantEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/assistant")
                .WithTags("Assistant")
                .WithOpenApi();

            group.MapGet("/status", GetStatusAsync)
                .WithSummary("Indica si el asistente de IA está configurado (sin exponer la API Key) y el modelo en uso.");
            group.MapPost("/ask", AskAsync)
                .WithSummary("Consulta al asistente (Google Gemini) con historial y, opcionalmente, un documento como contexto. 503 sin API Key, 429 por límite.");

            return app;
        }

        private static async Task<IResult> GetStatusAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetAssistantStatusQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> AskAsync(AskAssistantRequest request, IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<AskAssistantCommand>();
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        }
    }
}
