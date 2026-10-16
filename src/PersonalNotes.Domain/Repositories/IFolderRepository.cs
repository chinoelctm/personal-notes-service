using PersonalNotes.Domain.Entities;

namespace PersonalNotes.Domain.Repositories
{
    public interface IFolderRepository
    {
        Task AddAsync(Folder folder, CancellationToken ct = default);
        Task UpdateAsync(Folder folder, CancellationToken ct = default);

        /// <summary>Activa o no.</summary>
        Task<Folder?> GetByIdAsync(string id, CancellationToken ct = default);

        /// <summary>Carpetas activas ordenadas por nombre; el front arma el árbol.</summary>
        Task<List<Folder>> GetActiveAsync(CancellationToken ct = default);

        /// <summary>Todas (activas e inactivas), para el respaldo.</summary>
        Task<List<Folder>> GetAllAsync(CancellationToken ct = default);

        Task<bool> ExistsActiveByNameAsync(string? parentFolderId, string name, string? excludeId = null,
            CancellationToken ct = default);

        /// <summary>Desactiva las carpetas activas indicadas (borrado lógico en cascada).</summary>
        Task<long> DisableManyAsync(IReadOnlyCollection<string> ids, DateTime now, CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Folder> folders, CancellationToken ct = default);
    }
}
