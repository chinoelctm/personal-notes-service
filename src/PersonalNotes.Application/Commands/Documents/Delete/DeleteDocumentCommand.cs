using MediatR;

namespace PersonalNotes.Application.Commands.Documents.Delete
{
    /// <summary>Borra el documento de forma permanente (borrado físico).</summary>
    public record DeleteDocumentCommand(string Id) : IRequest;
}
