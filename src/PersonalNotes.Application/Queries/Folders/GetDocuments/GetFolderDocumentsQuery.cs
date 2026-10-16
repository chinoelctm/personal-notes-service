using MediatR;
using PersonalNotes.Application.Queries.Documents;

namespace PersonalNotes.Application.Queries.Folders.GetDocuments
{
    /// <summary>Documentos activos de la carpeta, el editado más recientemente primero.</summary>
    public record GetFolderDocumentsQuery(string FolderId) : IRequest<List<DocumentResponse>>;
}
