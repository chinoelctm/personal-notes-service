using MediatR;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Queries.Documents.Get
{
    public class GetDocumentsHandler : IRequestHandler<GetDocumentsQuery, List<DocumentResponse>>
    {
        private readonly IDocumentRepository _documents;
        private readonly IFolderHierarchyProvider _hierarchyProvider;

        public GetDocumentsHandler(IDocumentRepository documents, IFolderHierarchyProvider hierarchyProvider)
        {
            _documents = documents;
            _hierarchyProvider = hierarchyProvider;
        }

        public async Task<List<DocumentResponse>> Handle(GetDocumentsQuery query, CancellationToken ct)
        {
            var hierarchy = await _hierarchyProvider.GetAsync(ct);
            var documents = await _documents.GetActiveAsync(ct);

            // Solo los que cuelgan de una carpeta visible en el árbol
            return documents
                .Where(d => hierarchy.Contains(d.FolderId))
                .Select(DocumentResponse.From)
                .ToList();
        }
    }
}
