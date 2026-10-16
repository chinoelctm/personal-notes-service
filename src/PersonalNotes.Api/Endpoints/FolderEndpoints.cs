using Mapster;
using MediatR;
using PersonalNotes.Api.Models.Folder.AddFolder;
using PersonalNotes.Api.Models.Folder.UpdateFolder;
using PersonalNotes.Application.Commands.Folders.Add;
using PersonalNotes.Application.Commands.Folders.Delete;
using PersonalNotes.Application.Commands.Folders.Update;
using PersonalNotes.Application.Queries.Folders.Get;
using PersonalNotes.Application.Queries.Folders.GetDocuments;

namespace PersonalNotes.Api.Endpoints
{
    public static class FolderEndpoints
    {
        public static IEndpointRouteBuilder MapFolderEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/v1/folders")
                .WithTags("Folders")
                .WithOpenApi();

            group.MapGet("/", GetAsync)
                .WithSummary("Carpetas activas (lista plana con parentFolderId, nivel, ruta y cantidad de documentos).");
            group.MapPost("/", AddAsync)
                .WithSummary("Crea una carpeta en la raíz o dentro de otra, respetando la profundidad máxima del árbol.");
            group.MapPut("/{folderId}", UpdateAsync)
                .WithSummary("Renombra y/o mueve la carpeta con todo su contenido. 400 si crea un ciclo o supera la profundidad.");
            group.MapDelete("/{folderId}", DeleteAsync)
                .WithSummary("Elimina (desactiva) la carpeta, sus subcarpetas y sus documentos.");
            group.MapGet("/{folderId}/documents", GetDocumentsAsync)
                .WithSummary("Documentos activos de la carpeta, el editado más recientemente primero.");

            return app;
        }

        private static async Task<IResult> GetAsync(IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetFoldersQuery(), ct);
            return Results.Ok(result);
        }

        private static async Task<IResult> AddAsync(AddFolderRequest request, IMediator mediator, CancellationToken ct)
        {
            var command = request.Adapt<AddFolderCommand>();
            var folderId = await mediator.Send(command, ct);
            return Results.Created($"/api/v1/folders/{folderId}", new { id = folderId });
        }

        private static async Task<IResult> UpdateAsync(string folderId, UpdateFolderRequest request, IMediator mediator,
            CancellationToken ct)
        {
            var command = request.Adapt<UpdateFolderCommand>() with { Id = folderId };
            await mediator.Send(command, ct);
            return Results.NoContent();
        }

        private static async Task<IResult> DeleteAsync(string folderId, IMediator mediator, CancellationToken ct)
        {
            await mediator.Send(new DeleteFolderCommand(folderId), ct);
            return Results.NoContent();
        }

        private static async Task<IResult> GetDocumentsAsync(string folderId, IMediator mediator, CancellationToken ct)
        {
            var result = await mediator.Send(new GetFolderDocumentsQuery(folderId), ct);
            return Results.Ok(result);
        }
    }
}
