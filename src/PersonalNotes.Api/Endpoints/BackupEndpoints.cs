using MediatR;
using PersonalNotes.Application.Commands.Backup.Import;
using PersonalNotes.Application.DTOs;
using PersonalNotes.Application.Queries.Backup.Export;

namespace PersonalNotes.Api.Endpoints
{
    public static class BackupEndpoints
    {
        public static IEndpointRouteBuilder MapBackupEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/backup")
                .WithTags("Backup")
                .WithOpenApi();

            group.MapGet("/", ExportAsync)
                .WithSummary("Exporta todas las carpetas y documentos (incluidos los eliminados).");
            group.MapPost("/", ImportAsync)
                .WithSummary("Reemplaza todas las carpetas y documentos por el respaldo enviado (se valida completo antes).");

            return app;
        }

        private static async Task<IResult> ExportAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new ExportBackupQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> ImportAsync(BackupDto request, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new ImportBackupCommand(request), ct);
            return Results.Ok(result);
        }
    }
}
