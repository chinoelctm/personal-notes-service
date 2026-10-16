using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.DTOs
{
    /// <summary>Respaldo completo: todas las carpetas y documentos (incluidos los eliminados), con sus Id y fechas.</summary>
    /// <param name="ExportedOn">UTC.</param>
    public record BackupDto(
        int Version,
        DateTime ExportedOn,
        List<FolderBackupDto> Folders,
        List<DocumentBackupDto> Documents)
    {
        public const int CurrentVersion = 1;
    }

    public record FolderBackupDto(
        string Id,
        string Name,
        string? ParentFolderId,
        bool Enabled,
        DateTime CreatedOn,
        DateTime UpdatedOn)
    {
        public static FolderBackupDto From(Folder folder) => new(
            folder.Id, folder.Name, folder.ParentFolderId, folder.Enabled, folder.CreatedOn, folder.UpdatedOn);

        public Folder ToEntity() => Folder.Restore(Id, Name, ParentFolderId, Enabled, CreatedOn, UpdatedOn);
    }

    public record DocumentBackupDto(
        string Id,
        string FolderId,
        string? Title,
        string? Content,
        Priority Priority,
        bool Enabled,
        DateTime CreatedOn,
        DateTime UpdatedOn)
    {
        public static DocumentBackupDto From(Document document) => new(
            document.Id, document.FolderId, document.Title, document.Content, document.Priority, document.Enabled,
            document.CreatedOn, document.UpdatedOn);

        public Document ToEntity() =>
            Document.Restore(Id, FolderId, Title, Content, Priority, Enabled, CreatedOn, UpdatedOn);
    }
}
