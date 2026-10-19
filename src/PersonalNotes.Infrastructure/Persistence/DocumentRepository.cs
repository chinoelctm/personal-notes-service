using MongoDB.Driver;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Repositories;
using PersonalNotes.Domain.ValueObjects;
using PersonalNotes.Infrastructure.Context;

namespace PersonalNotes.Infrastructure.Persistence
{
    public class DocumentRepository : IDocumentRepository
    {
        private readonly IMongoCollection<Document> _documents;

        public DocumentRepository(PersonalNotesDbContext context)
        {
            _documents = context.Documents;
        }

        public async Task AddAsync(Document document, CancellationToken ct = default)
        {
            await _documents.InsertOneAsync(document, cancellationToken: ct).ConfigureAwait(false);
        }

        public async Task UpdateAsync(Document document, CancellationToken ct = default)
        {
            var filter = Builders<Document>.Filter.Eq(x => x.Id, document.Id);

            await _documents
                .ReplaceOneAsync(filter, document, cancellationToken: ct)
                .ConfigureAwait(false);
        }

        public async Task<Document?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _documents
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        // Última edición primero. Coincide con ix_documents_updated / ix_documents_folder_updated: sin orden en memoria
        private static readonly SortDefinition<Document> LastEditedFirst = Builders<Document>.Sort
            .Descending(x => x.UpdatedOn);

        public async Task<List<Document>> GetAllAsync(CancellationToken ct = default)
        {
            return await _documents
                .Find(Builders<Document>.Filter.Empty)
                .Sort(LastEditedFirst)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Document>> GetByFolderAsync(string folderId, CancellationToken ct = default)
        {
            return await _documents
                .Find(Builders<Document>.Filter.Eq(x => x.FolderId, folderId))
                .Sort(LastEditedFirst)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<DocumentStamp>> GetStampsAsync(CancellationToken ct = default)
        {
            // Solo carpeta, prioridad y fecha: no trae el contenido (que puede pesar varios MB por las imágenes)
            return await _documents
                .Find(Builders<Document>.Filter.Empty)
                .Sort(LastEditedFirst)
                .Project(x => new DocumentStamp(x.Id, x.FolderId, x.Priority, x.UpdatedOn))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
        {
            var result = await _documents
                .DeleteOneAsync(Builders<Document>.Filter.Eq(x => x.Id, id), ct)
                .ConfigureAwait(false);

            return result.DeletedCount > 0;
        }

        public async Task<long> DeleteByFoldersAsync(IReadOnlyCollection<string> folderIds, CancellationToken ct = default)
        {
            if (folderIds.Count == 0) return 0;

            var result = await _documents
                .DeleteManyAsync(Builders<Document>.Filter.In(x => x.FolderId, folderIds), ct)
                .ConfigureAwait(false);

            return result.DeletedCount;
        }

        public async Task ReplaceAllAsync(IReadOnlyCollection<Document> documents, CancellationToken ct = default)
        {
            await _documents.DeleteManyAsync(Builders<Document>.Filter.Empty, ct).ConfigureAwait(false);
            if (documents.Count > 0)
                await _documents.InsertManyAsync(documents, cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
