using MongoDB.Driver;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;
using PersonalNotes.Infrastructure.Context;

namespace PersonalNotes.Infrastructure.Persistence
{
    public class FolderRepository : IFolderRepository
    {
        private readonly IMongoCollection<Folder> _folders;

        public FolderRepository(PersonalNotesDbContext context)
        {
            _folders = context.Folders;
        }

        public async Task AddAsync(Folder folder, CancellationToken ct = default)
        {
            try
            {
                await _folders.InsertOneAsync(folder, cancellationToken: ct).ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw DuplicatedName();
            }
        }

        public async Task UpdateAsync(Folder folder, CancellationToken ct = default)
        {
            var filter = Builders<Folder>.Filter.Eq(x => x.Id, folder.Id);

            try
            {
                await _folders
                    .ReplaceOneAsync(filter, folder, cancellationToken: ct)
                    .ConfigureAwait(false);
            }
            catch (MongoWriteException ex) when (ex.WriteError?.Category == ServerErrorCategory.DuplicateKey)
            {
                throw DuplicatedName();
            }
        }

        // El índice único (padre, nombre) resuelve las altas/ediciones simultáneas con el mismo nombre
        private static ConflictException DuplicatedName() =>
            new("Ya existe una carpeta con ese nombre en la misma ubicación.");

        public async Task<Folder?> GetByIdAsync(string id, CancellationToken ct = default)
        {
            return await _folders
                .Find(x => x.Id == id)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);
        }

        // Mismo orden que ix_folders_active_name: se resuelve con el índice
        private static readonly SortDefinition<Folder> NameOrder = Builders<Folder>.Sort
            .Ascending(x => x.Enabled)
            .Ascending(x => x.NormalizedName);

        public async Task<List<Folder>> GetActiveAsync(CancellationToken ct = default)
        {
            return await _folders
                .Find(Builders<Folder>.Filter.Eq(x => x.Enabled, true))
                .Sort(NameOrder)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<List<Folder>> GetAllAsync(CancellationToken ct = default)
        {
            return await _folders
                .Find(Builders<Folder>.Filter.Empty)
                .Sort(NameOrder)
                .ToListAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<bool> ExistsActiveByNameAsync(string? parentFolderId, string name, string? excludeId = null,
            CancellationToken ct = default)
        {
            // Incluye enabled: true para que Mongo pueda usar el índice parcial ux_folders_parent_name
            var filter = Builders<Folder>.Filter.And(
                Builders<Folder>.Filter.Eq(x => x.ParentFolderId, parentFolderId),
                Builders<Folder>.Filter.Eq(x => x.NormalizedName, Folder.Normalize(name)),
                Builders<Folder>.Filter.Eq(x => x.Enabled, true),
                excludeId is null
                    ? Builders<Folder>.Filter.Empty
                    : Builders<Folder>.Filter.Ne(x => x.Id, excludeId));

            return await _folders
                .Find(filter)
                .Limit(1)
                .AnyAsync(ct)
                .ConfigureAwait(false);
        }

        public async Task<long> DisableManyAsync(IReadOnlyCollection<string> ids, DateTime now,
            CancellationToken ct = default)
        {
            if (ids.Count == 0) return 0;

            var filter = Builders<Folder>.Filter.And(
                Builders<Folder>.Filter.In(x => x.Id, ids),
                Builders<Folder>.Filter.Eq(x => x.Enabled, true));
            var update = Builders<Folder>.Update
                .Set(x => x.Enabled, false)
                .Set(x => x.UpdatedOn, now);

            var result = await _folders
                .UpdateManyAsync(filter, update, cancellationToken: ct)
                .ConfigureAwait(false);

            return result.ModifiedCount;
        }

        public async Task ReplaceAllAsync(IReadOnlyCollection<Folder> folders, CancellationToken ct = default)
        {
            await _folders.DeleteManyAsync(Builders<Folder>.Filter.Empty, ct).ConfigureAwait(false);
            if (folders.Count > 0)
                await _folders.InsertManyAsync(folders, cancellationToken: ct).ConfigureAwait(false);
        }
    }
}
