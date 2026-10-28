using MediatR;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Commands.Documents.Add
{
    /// <param name="Content">HTML del editor enriquecido.</param>
    /// <param name="Type">Rich (por defecto), Html o Markdown.</param>
    public record AddDocumentCommand(string FolderId, string? Title, string? Content, Priority Priority,
        DocumentType? Type = null)
        : IRequest<AddDocumentResponse>;

    /// <param name="UpdatedOn">Fecha de publicación (UTC) asignada por la API.</param>
    public record AddDocumentResponse(string Id, DateTime UpdatedOn);
}
