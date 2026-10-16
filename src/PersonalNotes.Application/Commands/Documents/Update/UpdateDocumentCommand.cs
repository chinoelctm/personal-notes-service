using MediatR;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Commands.Documents.Update
{
    /// <summary>Publica la versión editada del documento; cambiar FolderId lo mueve de carpeta.</summary>
    public record UpdateDocumentCommand(string Id, string FolderId, string? Title, string? Content, Priority Priority)
        : IRequest<UpdateDocumentResponse>;

    /// <param name="UpdatedOn">Fecha de publicación (UTC) asignada por la API.</param>
    public record UpdateDocumentResponse(string Id, DateTime UpdatedOn);
}
