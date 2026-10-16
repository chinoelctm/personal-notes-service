using MediatR;

namespace PersonalNotes.Application.Commands.Documents.Delete
{
    /// <summary>Desactiva el documento (borrado lógico): deja de aparecer en el árbol, el buscador y el dashboard.</summary>
    public record DeleteDocumentCommand(string Id) : IRequest;
}
