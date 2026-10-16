using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Domain.Repositories
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document, CancellationToken ct = default);
        Task UpdateAsync(Document document, CancellationToken ct = default);

        /// <summary>Activo o no.</summary>
        Task<Document?> GetByIdAsync(string id, CancellationToken ct = default);

        /// <summary>Documentos activos, el editado más recientemente primero.</summary>
        Task<List<Document>> GetActiveAsync(CancellationToken ct = default);

        /// <summary>Documentos activos de una carpeta, el editado más recientemente primero.</summary>
        Task<List<Document>> GetActiveByFolderAsync(string folderId, CancellationToken ct = default);

        /// <summary>Prioridad y fecha de edición de los documentos activos (sin título ni contenido).</summary>
        Task<List<DocumentStamp>> GetActiveStampsAsync(CancellationToken ct = default);

        /// <summary>Todos (activos e inactivos), para el respaldo.</summary>
        Task<List<Document>> GetAllAsync(CancellationToken ct = default);

        /// <summary>Desactiva los documentos activos de las carpetas indicadas.</summary>
        Task<long> DisableByFoldersAsync(IReadOnlyCollection<string> folderIds, DateTime now,
            CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Document> documents, CancellationToken ct = default);
    }
}
