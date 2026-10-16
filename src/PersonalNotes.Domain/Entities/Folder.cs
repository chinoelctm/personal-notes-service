using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Domain.Entities
{
    /// <summary>
    /// Carpeta del árbol de documentos. Sin <see cref="ParentFolderId"/> es una carpeta raíz. Eliminarla la
    /// desactiva (junto con sus subcarpetas y documentos), así un respaldo anterior sigue siendo coherente.
    /// </summary>
    public class Folder
    {
        public const int MaxNameLength = 80;

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; } = null!;

        [BsonElement("name")] public string Name { get; private set; } = null!;

        // Nombre en minúsculas, para el índice único (no puede haber dos hermanas activas con el mismo nombre)
        [BsonElement("normalizedName")] public string NormalizedName { get; private set; } = null!;

        // Las raíces guardan null explícito: así el índice único (parentFolderId, normalizedName) también las cubre
        [BsonElement("parentFolderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentFolderId { get; private set; }

        [BsonElement("enabled")] public bool Enabled { get; private set; }

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime UpdatedOn { get; private set; }

        private Folder()
        {
        }

        private Folder(string id, string name, string? parentFolderId, bool enabled, DateTime createdOn,
            DateTime updatedOn)
        {
            Id = id;
            Name = name;
            NormalizedName = Normalize(name);
            ParentFolderId = parentFolderId;
            Enabled = enabled;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }

        public static Folder Create(string name, string? parentFolderId, DateTime now)
        {
            ValidateName(name);
            return new Folder(ObjectIds.New(), name.Trim(), ValidateParent(parentFolderId), enabled: true, now, now);
        }

        /// <summary>Carpeta restaurada desde un respaldo, con su Id y fechas originales.</summary>
        public static Folder Restore(string id, string name, string? parentFolderId, bool enabled, DateTime createdOn,
            DateTime updatedOn)
        {
            if (!ObjectIds.IsValid(id))
                throw new DomainException("El Id de la carpeta no es válido.");

            ValidateName(name);
            var parent = ValidateParent(parentFolderId);
            if (parent == id)
                throw new BusinessRuleException("Una carpeta no puede ser su propia carpeta padre.");

            return new Folder(id, name.Trim(), parent, enabled, ToUtc(createdOn), ToUtc(updatedOn));
        }

        public bool IsRoot => ParentFolderId is null;

        /// <summary>Renombra y/o mueve la carpeta. La profundidad y los ciclos se validan con el árbol completo.</summary>
        public void Update(string name, string? parentFolderId, DateTime now)
        {
            if (!Enabled)
                throw new BusinessRuleException("La carpeta fue eliminada; no se puede modificar.");

            ValidateName(name);
            var parent = ValidateParent(parentFolderId);
            if (parent == Id)
                throw new BusinessRuleException("Una carpeta no puede ser su propia carpeta padre.");

            Name = name.Trim();
            NormalizedName = Normalize(Name);
            ParentFolderId = parent;
            UpdatedOn = now;
        }

        public void Disable(DateTime now)
        {
            if (!Enabled) return;
            Enabled = false;
            UpdatedOn = now;
        }

        public static string Normalize(string name) => name.Trim().ToLowerInvariant();

        private static string? ValidateParent(string? parentFolderId)
        {
            if (string.IsNullOrWhiteSpace(parentFolderId)) return null;
            if (!ObjectIds.IsValid(parentFolderId.Trim()))
                throw new DomainException("La carpeta padre no es válida.");
            return parentFolderId.Trim();
        }

        private static void ValidateName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("El nombre de la carpeta es obligatorio.");
            if (name.Trim().Length > MaxNameLength)
                throw new DomainException($"El nombre no puede superar los {MaxNameLength} caracteres.");
        }

        // Mongo guarda en UTC: una fecha de un respaldo sin zona se interpreta como UTC, no como hora local
        private static DateTime ToUtc(DateTime date) => date.Kind switch
        {
            DateTimeKind.Utc => date,
            DateTimeKind.Local => date.ToUniversalTime(),
            _ => DateTime.SpecifyKind(date, DateTimeKind.Utc)
        };
    }
}
