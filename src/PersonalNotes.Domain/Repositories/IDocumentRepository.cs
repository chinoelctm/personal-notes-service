using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Domain.Repositories
{
    public interface IDocumentRepository
    {
        Task AddAsync(Document document, CancellationToken ct = default);
        Task UpdateAsync(Document document, CancellationToken ct = default);
        Task<Document?> GetByIdAsync(string id, CancellationToken ct = default);

        /// <summary>Todos los documentos, el editado más recientemente primero.</summary>
        Task<List<Document>> GetAllAsync(CancellationToken ct = default);

        /// <summary>Documentos de una carpeta, el editado más recientemente primero.</summary>
        Task<List<Document>> GetByFolderAsync(string folderId, CancellationToken ct = default);

        /// <summary>Carpeta, prioridad y fecha de edición de cada documento (sin título ni contenido).</summary>
        Task<List<DocumentStamp>> GetStampsAsync(CancellationToken ct = default);

        /// <summary>Borra físicamente el documento. false si no existía.</summary>
        Task<bool> DeleteAsync(string id, CancellationToken ct = default);

        /// <summary>Borra físicamente los documentos de las carpetas indicadas.</summary>
        Task<long> DeleteByFoldersAsync(IReadOnlyCollection<string> folderIds, CancellationToken ct = default);

        Task ReplaceAllAsync(IReadOnlyCollection<Document> documents, CancellationToken ct = default);
    }
}
