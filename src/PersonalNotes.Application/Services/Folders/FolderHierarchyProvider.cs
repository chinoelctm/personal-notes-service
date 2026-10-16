using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Services.Folders
{
    public class FolderHierarchyProvider : IFolderHierarchyProvider
    {
        private readonly IFolderRepository _repository;

        public FolderHierarchyProvider(IFolderRepository repository)
        {
            _repository = repository;
        }

        public async Task<FolderHierarchy> GetAsync(CancellationToken ct = default)
        {
            var folders = await _repository.GetActiveAsync(ct);
            return new FolderHierarchy(folders);
        }
    }
}
