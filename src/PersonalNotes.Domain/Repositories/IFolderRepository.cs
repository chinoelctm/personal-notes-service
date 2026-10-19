using PersonalNotes.Domain.Entities;

namespace PersonalNotes.Domain.Repositories
{
    public interface IFolderRepository
    {
        Task AddAsync(Folder folder, CancellationToken ct = default);
        Task UpdateAsync(Folder folder, CancellationToken ct = default);
        Task<Folder?> GetByIdAsync(string id, CancellationToken ct = default);

        /// <summary>Todas las carpetas ordenadas por nombre; el front arma el árbol.</summary>
        Task<List<Folder>> GetAllAsync(CancellationToken ct = default);

        Task<bool> ExistsByNameAsync(string? parentFolderId, string name, string? excludeId = null,
            CancellationToken ct = default);

        /// <summary>Borra físicamente las carpetas indicadas (cascada armada con el árbol).</summary>
        Task<long> DeleteManyAsync(IReadOnlyCollection<string> ids, CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Folder> folders, CancellationToken ct = default);
    }
}
