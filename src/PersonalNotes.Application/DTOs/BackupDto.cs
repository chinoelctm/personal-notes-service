using System.Text.Json.Serialization;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.DTOs
{
    /// <summary>Respaldo completo: todas las carpetas y documentos, con sus Id y fechas.</summary>
    /// <param name="Version">1 = con borrado lógico (campo enabled); 2 = borrado físico (sin enabled).</param>
    /// <param name="ExportedOn">UTC.</param>
    public record BackupDto(
        int Version,
        DateTime ExportedOn,
        List<FolderBackupDto> Folders,
        List<DocumentBackupDto> Documents)
    {
        public const int CurrentVersion = 2;
    }

    /// <param name="Enabled">Solo en respaldos de la versión 1: false = carpeta eliminada, se omite al importar.</param>
    public record FolderBackupDto(
        string Id,
        string Name,
        string? ParentFolderId,
        DateTime CreatedOn,
        DateTime UpdatedOn,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] bool? Enabled = null)
    {
        public static FolderBackupDto From(Folder folder) => new(
            folder.Id, folder.Name, folder.ParentFolderId, folder.CreatedOn, folder.UpdatedOn);

        [JsonIgnore]
        public bool IsDeleted => Enabled == false;

        public Folder ToEntity() => Folder.Restore(Id, Name, ParentFolderId, CreatedOn, UpdatedOn);
    }

    /// <param name="Enabled">Solo en respaldos de la versión 1: false = documento eliminado, se omite al importar.</param>
    public record DocumentBackupDto(
        string Id,
        string FolderId,
        string? Title,
        string? Content,
        Priority Priority,
        DateTime CreatedOn,
        DateTime UpdatedOn,
        [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] bool? Enabled = null)
    {
        public static DocumentBackupDto From(Document document) => new(
            document.Id, document.FolderId, document.Title, document.Content, document.Priority,
            document.CreatedOn, document.UpdatedOn);

        [JsonIgnore]
        public bool IsDeleted => Enabled == false;

        public Document ToEntity() =>
            Document.Restore(Id, FolderId, Title, Content, Priority, CreatedOn, UpdatedOn);
    }
}
