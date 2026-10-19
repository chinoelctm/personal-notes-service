using MongoDB.Bson;
using MongoDB.Driver;
using PersonalNotes.Domain.Entities;

namespace PersonalNotes.Infrastructure.Context
{
    public sealed class PersonalNotesDbContext
    {
        // Solo los índices con estos prefijos los administra la aplicación (se crean y se eliminan si dejan de usarse)
        private static readonly string[] ManagedIndexPrefixes = ["ix_", "ux_"];

        private const string FoldersCollection = "folders";
        private const string DocumentsCollection = "documents";

        public IMongoDatabase Database { get; }

        public IMongoCollection<Folder> Folders { get; }
        public IMongoCollection<Document> Documents { get; }

        public PersonalNotesDbContext(IMongoClient client, string databaseName)
        {
            Database = client.GetDatabase(databaseName);

            Folders = Database.GetCollection<Folder>(FoldersCollection);
            Documents = Database.GetCollection<Document>(DocumentsCollection);
        }

        /// <summary>
        /// Migración del borrado lógico al físico: borra los registros que quedaron con <c>enabled: false</c> y quita
        /// el campo de los demás. Es idempotente; tras la primera ejecución no encuentra nada que cambiar.
        /// </summary>
        public async Task<(long Deleted, long Cleaned)> RemoveLogicalDeletesAsync(CancellationToken ct = default)
        {
            long deleted = 0, cleaned = 0;
            foreach (var name in new[] { DocumentsCollection, FoldersCollection })
            {
                var collection = Database.GetCollection<BsonDocument>(name);

                var removed = await collection.DeleteManyAsync(new BsonDocument("enabled", false), ct);
                var updated = await collection.UpdateManyAsync(
                    new BsonDocument("enabled", new BsonDocument("$exists", true)),
                    new BsonDocument("$unset", new BsonDocument("enabled", "")),
                    cancellationToken: ct);

                deleted += removed.DeletedCount;
                cleaned += updated.ModifiedCount;
            }
            return (deleted, cleaned);
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
            // ExistsByName + no puede haber dos carpetas hermanas con el mismo nombre
            // (las raíces comparten parentFolderId: null)
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys
                    .Ascending(x => x.ParentFolderId)
                    .Ascending(x => x.NormalizedName),
                new CreateIndexOptions { Name = "ux_folders_name_in_parent", Unique = true }),

            // GetAll: todas las carpetas ordenadas por nombre (árbol y respaldo)
            new CreateIndexModel<Folder>(
                Builders<Folder>.IndexKeys.Ascending(x => x.NormalizedName),
                new CreateIndexOptions { Name = "ix_folders_name" })
        ];

        private static IReadOnlyList<CreateIndexModel<Document>> DocumentIndexes() =>
        [
            // GetAll y GetStamps: orden por última edición (buscador, dashboard, asistente, respaldo)
            new CreateIndexModel<Document>(
                Builders<Document>.IndexKeys.Descending(x => x.UpdatedOn),
                new CreateIndexOptions { Name = "ix_documents_updated" }),

            // GetByFolder (vista de directorio ya ordenada) y DeleteByFolders (folderId $in)
            new CreateIndexModel<Document>(
                Builders<Document>.IndexKeys
                    .Ascending(x => x.FolderId)
                    .Descending(x => x.UpdatedOn),
                new CreateIndexOptions { Name = "ix_documents_folder_updated_on" })
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
