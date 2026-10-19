using MediatR;

namespace PersonalNotes.Application.Queries.Documents.Get
{
    /// <summary>
    /// Todos los documentos (con contenido), el editado más recientemente primero. El front los guarda
    /// en su caché local para el buscador, el árbol y las vistas previas.
    /// </summary>
    public record GetDocumentsQuery : IRequest<List<DocumentResponse>>;
}
