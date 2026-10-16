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

        // Última edición primero. Coincide con los índices ix_documents_*_updated, así Mongo no ordena en memoria
        private static readonly SortDefinition<Document> LastEditedFirst = Builders<Document>.Sort
            .Descending(x => x.UpdatedOn);

        private static readonly FilterDefinition<Document> Active = Builders<Document>.Filter.Eq(x => x.Enabled, true);

        public async Task<List<Document>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _documents
                .Find(Active)
                .Sort(LastEditedFirst)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Document>> GetActiveByFolderAsync(string folderId, CancellationToken ct = default)
        {
            var filter = Builders<Document>.Filter.And(
                Builders<Document>.Filter.Eq(x => x.FolderId, folderId),
                Active);

            return await _documents
                .Find(filter)
                .Sort(LastEditedFirst)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<DocumentStamp>> GetActiveStampsAsync(CancellationToken ct = default)
        {
            // Solo carpeta, prioridad y fecha: no trae el contenido (que puede pesar varios MB por las imágenes)
            return await _documents
                .Find(Active)
                .Sort(LastEditedFirst)
                .Project(x => new DocumentStamp(x.Id, x.FolderId, x.Priority, x.UpdatedOn))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Document>> GetAllAsync(CancellationToken ct = default)
        {
            // Mismo orden que ix_documents_active_updated (enabled, updatedOn ↓)
            return await _documents
                .Find(Builders<Document>.Filter.Empty)
                .Sort(Builders<Document>.Sort.Ascending(x => x.Enabled).Descending(x => x.UpdatedOn))
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<long> DisableByFoldersAsync(IReadOnlyCollection<string> folderIds, DateTime now,
            CancellationToken ct = default)
        {
            if (folderIds.Count == 0) return 0;

            var filter = Builders<Document>.Filter.And(
                Builders<Document>.Filter.In(x => x.FolderId, folderIds),
                Active);
            var update = Builders<Document>.Update
                .Set(x => x.Enabled, false)
                .Set(x => x.UpdatedOn, now);

            var result = await _documents
                .UpdateManyAsync(filter, update, cancellationToken: ct)
                .ConfigureAwait(false);

            return result.ModifiedCount;
        }

        public async Task ReplaceAllAsync(IReadOnlyCollection<Document> documents, CancellationToken ct = default)
        {
            await _documents.DeleteManyAsync(Builders<Document>.Filter.Empty, ct).ConfigureAwait(false);
            if (documents.Count > 0)
                await _documents.InsertManyAsync(documents, cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
