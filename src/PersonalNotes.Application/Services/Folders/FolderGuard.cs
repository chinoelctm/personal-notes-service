using Microsoft.Extensions.Logging;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Services.Folders
{
    /// <summary>Un documento solo se puede crear o mover a una carpeta que exista.</summary>
    public class FolderGuard : IFolderGuard
    {
        private readonly IFolderRepository _repository;
        private readonly ILogger<FolderGuard> _logger;

        public FolderGuard(IFolderRepository repository, ILogger<FolderGuard> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task EnsureExistsAsync(string folderId, CancellationToken ct = default)
        {
            var folder = await _repository.GetByIdAsync(folderId, ct);
            if (folder is not null) return;

            _logger.LogError("No existe la carpeta {FolderId}.", folderId);
            throw new NotFoundException("La carpeta no existe.");
        }
    }
}
