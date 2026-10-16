using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Domain.Entities
{
    /// <summary>
    /// Documento (nota) dentro de una carpeta. <see cref="Content"/> es HTML del editor enriquecido y puede
    /// traer imágenes incrustadas en Base64. <see cref="UpdatedOn"/> siempre tiene valor (al crear es igual a
    /// <see cref="CreatedOn"/>) porque ordena los listados por última edición.
    /// </summary>
    public class Document
    {
        public const int MaxTitleLength = 200;

        // Un documento de Mongo admite 16 MB; se deja margen para el resto de los campos
        public const int MaxContentLength = 12_000_000;

        public const string DefaultTitle = "Sin título";

        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; private set; } = null!;

        [BsonElement("folderId")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string FolderId { get; private set; } = null!;

        [BsonElement("title")] public string Title { get; private set; } = null!;

        [BsonElement("content")] public string Content { get; private set; } = null!;

        [BsonElement("priority")]
        [BsonRepresentation(BsonType.String)]
        public Priority Priority { get; private set; }

        [BsonElement("enabled")] public bool Enabled { get; private set; }

        [BsonElement("createdOn")] public DateTime CreatedOn { get; private set; }

        [BsonElement("updatedOn")] public DateTime UpdatedOn { get; private set; }

        private Document()
        {
        }

        private Document(string id, string folderId, string title, string content, Priority priority, bool enabled,
            DateTime createdOn, DateTime updatedOn)
        {
            Id = id;
            FolderId = folderId;
            Title = title;
            Content = content;
            Priority = priority;
            Enabled = enabled;
            CreatedOn = createdOn;
            UpdatedOn = updatedOn;
        }

        public static Document Create(string folderId, string? title, string? content, Priority priority, DateTime now)
        {
            return new Document(ObjectIds.New(), ValidateFolder(folderId), NormalizeTitle(title),
                ValidateContent(content), ValidatePriority(priority), enabled: true, now, now);
        }

        /// <summary>Documento restaurado desde un respaldo, con su Id y fechas originales.</summary>
        public static Document Restore(string id, string folderId, string? title, string? content, Priority priority,
            bool enabled, DateTime createdOn, DateTime updatedOn)
        {
            if (!ObjectIds.IsValid(id))
                throw new DomainException("El Id del documento no es válido.");

            return new Document(id, ValidateFolder(folderId), NormalizeTitle(title), ValidateContent(content),
                ValidatePriority(priority), enabled, ToUtc(createdOn), ToUtc(updatedOn));
        }

        /// <summary>Publica una nueva versión (título, contenido, prioridad) y/o mueve el documento de carpeta.</summary>
        public void Update(string folderId, string? title, string? content, Priority priority, DateTime now)
        {
            if (!Enabled)
                throw new BusinessRuleException("El documento fue eliminado; no se puede editar.");

            FolderId = ValidateFolder(folderId);
            Title = NormalizeTitle(title);
            Content = ValidateContent(content);
            Priority = ValidatePriority(priority);
            UpdatedOn = now;
        }

        public void Disable(DateTime now)
        {
            if (!Enabled) return;
            Enabled = false;
            UpdatedOn = now;
        }

        /// <summary>Título vacío = "Sin título": el editor permite publicar un borrador sin nombre.</summary>
        public static string NormalizeTitle(string? title)
        {
            var trimmed = (title ?? string.Empty).Trim();
            if (trimmed.Length > MaxTitleLength)
                throw new DomainException($"El título no puede superar los {MaxTitleLength} caracteres.");
            return trimmed.Length == 0 ? DefaultTitle : trimmed;
        }

        private static string ValidateContent(string? content)
        {
            var value = content ?? string.Empty;
            if (value.Length > MaxContentLength)
                throw new DomainException("El contenido es demasiado grande; reduce el tamaño de las imágenes.");
            return value;
        }

        private static string ValidateFolder(string folderId)
        {
            if (!ObjectIds.IsValid(folderId?.Trim()))
                throw new DomainException("La carpeta del documento no es válida.");
            return folderId!.Trim();
        }

        private static Priority ValidatePriority(Priority priority)
        {
            if (!Enum.IsDefined(priority))
                throw new DomainException("La prioridad no es válida.");
            return priority;
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
