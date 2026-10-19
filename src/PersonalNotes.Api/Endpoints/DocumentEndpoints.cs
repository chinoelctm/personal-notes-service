using Mapster;
using MediatR;
using PersonalNotes.Api.Models.Document.AddDocument;
using PersonalNotes.Api.Models.Document.UpdateDocument;
using PersonalNotes.Application.Commands.Documents.Add;
using PersonalNotes.Application.Commands.Documents.Delete;
using PersonalNotes.Application.Commands.Documents.Update;
using PersonalNotes.Application.Queries.Documents.Get;
using PersonalNotes.Application.Queries.Documents.GetById;

namespace PersonalNotes.Api.Endpoints
{
    public static class DocumentEndpoints
    {
        public static IEndpointRouteBuilder MapDocumentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/documents")
                .WithTags("Documents")
                .WithOpenApi();

            group.MapGet("/", GetAsync)
                .WithSummary("Todos los documentos con su contenido, el editado más recientemente primero.");
            group.MapGet("/{documentId}", GetByIdAsync)
                .WithSummary("Documento por Id.");
            group.MapPost("/", AddAsync)
                .WithSummary("Publica un documento nuevo en una carpeta. Las fechas de auditoría se asignan en UTC.");
            group.MapPut("/{documentId}", UpdateAsync)
                .WithSummary("Publica la versión editada (título, contenido, prioridad) y/o mueve el documento de carpeta.");
            group.MapDelete("/{documentId}", DeleteAsync)
                .WithSummary("Borra el documento de forma permanente.");

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetDocumentsQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> GetByIdAsync(string documentId, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetDocumentByIdQuery(documentId), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> AddAsync(AddDocumentRequest request, IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<AddDocumentCommand>();
            var result = await mediator.Send(command, ct);
            return Results.Created($"/api/v1/documents/{result.Id}", result);
        }

        private static async Task<IResult> UpdateAsync(string documentId, UpdateDocumentRequest request,
            IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<UpdateDocumentCommand>() with { Id = documentId };
            var result = await mediator.Send(command, ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> DeleteAsync(string documentId, IMediator mediator, CancellationToken ct)
        {
            await mediator.Send(new DeleteDocumentCommand(documentId), ct);
            return Results.NoContent();
        }
    }
}
