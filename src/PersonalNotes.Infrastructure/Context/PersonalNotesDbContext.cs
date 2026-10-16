using MongoDB.Driver;
using PersonalNotes.Domain.Entities;

namespace PersonalNotes.Infrastructure.Context
{
    public sealed class PersonalNotesDbContext
    {
        // Solo los índices con estos prefijos los administra la aplicación (se crean y se eliminan si dejan de usarse)
        private static readonly string[] ManagedIndexPrefixes = ["ix_", "ux_"];

        public IMongoDatabase Database { get; }

        public IMongoCollection<Folder> Folders { get; }
        public IMongoCollection<Document> Documents { get; }

        public PersonalNotesDbContext(IMongoClient client, string databaseName)
        {
            Database = client.GetDatabase(databaseName);

            Folders = Database.GetCollection<Folder>("folders");
            Documents = Database.GetCollection<Document>("documents");
        }

        /// <summary>
        /// Deja cada colección exactamente con los índices definidos aquí: crea los que faltan y elimina
        /// los propios que ya no se usan. Es idempotente; se ejecuta en cada arranque.
        /// </summary>
        public async Task EnsureIndexesAsync(CancellationToken ct = default)
        {
            await SyncIndexesAsync(Folders, FolderIndexes(), ct);
            await SyncIndexesAsync(Documents, DocumentIndexes(), ct);
        }

        private static IReadOnlyList<CreateIndexModel<Folder>> FolderIndexes() =>
        [
            // ExistsActiveByName + no puede haber dos carpetas hermanas activas con el mismo nombre
            // (las raíces comparten parentFolderId: null)
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys
                    .Ascending(x => x.ParentFolderId)
                    .Ascending(x => x.NormalizedName),
                new CreateIndexOptions<Folder>
                {
                    Name = "ux_folders_parent_name",
                    Unique = true,
                    PartialFilterExpression = Builders<Folder>.Filter.Eq(x => x.Enabled, true)
                }),

            // GetActive (filtro enabled + orden por nombre) y GetAll del respaldo (mismo orden, sin filtro);
            // DisableMany filtra por _id
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys
                    .Ascending(x => x.Enabled)
                    .Ascending(x => x.NormalizedName),
                new CreateIndexOptions { Name = "ix_folders_active_name" })
        ];

        private static IReadOnlyList<CreateIndexModel<Document>> DocumentIndexes() =>
        [
            // GetActive y GetActiveStamps (filtro enabled + orden por última edición) y GetAll del respaldo
            new CreateIndexModel<Document>(
                Builders<Document>.IndexKeys
                    .Ascending(x => x.Enabled)
                    .Descending(x => x.UpdatedOn),
                new CreateIndexOptions { Name = "ix_documents_active_updated" }),

            // GetActiveByFolder (vista de directorio ya ordenada) y DisableByFolders (folderId $in + enabled)
            new CreateIndexModel<Document>(
                Builders<Document>.IndexKeys
                    .Ascending(x => x.FolderId)
                    .Ascending(x => x.Enabled)
                    .Descending(x => x.UpdatedOn),
                new CreateIndexOptions { Name = "ix_documents_folder_updated" })
        ];

        private static async Task SyncIndexesAsync<T>(IMongoCollection<T> collection,
            IReadOnlyList<CreateIndexModel<T>> desired, CancellationToken ct)
        {
            // Primero se crean los nuevos para no dejar la colección sin índice entre medio
            await collection.Indexes.CreateManyAsync(desired, ct);

            var desiredNames = desired.Select(i => i.Options.Name).ToHashSet();
            using var cursor = await collection.Indexes.ListAsync(ct);
            var existingNames = (await cursor.ToListAsync(ct)).Select(i => i["name"].AsString);

            var obsolete = existingNames.Where(name =>
                ManagedIndexPrefixes.Any(name.StartsWith) && !desiredNames.Contains(name));

            foreach (var name in obsolete)
                await collection.Indexes.DropOneAsync(name, ct);
        }
    }
}
