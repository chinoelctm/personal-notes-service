using MediatR;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Queries.Folders.Get
{
    public class GetFoldersHandler : IRequestHandler<GetFoldersQuery, List<FolderResponse>>
    {
        private readonly IFolderHierarchyProvider _hierarchyProvider;
        private readonly IDocumentRepository _documents;

        public GetFoldersHandler(IFolderHierarchyProvider hierarchyProvider, IDocumentRepository documents)
        {
            _hierarchyProvider = hierarchyProvider;
            _documents = documents;
        }

        public async Task<List<FolderResponse>> Handle(GetFoldersQuery query, CancellationToken ct)
        {
            var hierarchy = await _hierarchyProvider.GetAsync(ct);
            var stamps = await _documents.GetActiveStampsAsync(ct);
            var countByFolder = stamps
                .GroupBy(s => s.FolderId)
                .ToDictionary(g => g.Key, g => g.Count());

            // Lista plana ordenada por nombre (orden del índice); el front arma el árbol con ParentFolderId
            return hierarchy.Folders
                .OrderBy(f => f.NormalizedName, StringComparer.Ordinal)
                .Select(f =>
                {
                    var path = hierarchy.PathOf(f.Id);
                    return new FolderResponse(
                        f.Id,
                        f.Name,
                        f.ParentFolderId is not null && hierarchy.Contains(f.ParentFolderId) ? f.ParentFolderId : null,
                        path.Count,
                        path.Select(p => p.Name).ToList(),
                        countByFolder.GetValueOrDefault(f.Id),
                        f.CreatedOn,
                        f.UpdatedOn);
                })
                .ToList();
        }
    }
}
