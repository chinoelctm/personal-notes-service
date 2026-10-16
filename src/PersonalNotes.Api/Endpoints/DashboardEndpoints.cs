using MediatR;
using PersonalNotes.Application.Queries.Dashboard.Get;

namespace PersonalNotes.Api.Endpoints
{
    public static class DashboardEndpoints
    {
        public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/dashboard")
                .WithTags("Dashboard")
                .WithOpenApi();

            group.MapGet("/", GetAsync)
                .WithSummary("Estadísticas del inicio: total de notas y carpetas, desglose por prioridad y editadas recientemente.");

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetDashboardQuery(), ct);
            return Results.Ok(result);
        }
    }
}
