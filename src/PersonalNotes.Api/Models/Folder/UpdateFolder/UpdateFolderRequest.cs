namespace PersonalNotes.Api.Models.Folder.UpdateFolder
{
    /// <param name="Name">Nuevo nombre.</param>
    /// <param name="ParentFolderId">Nueva carpeta padre (null = raíz). Igual a la actual si solo se renombra.</param>
    public record UpdateFolderRequest(
        string Name,
        string? ParentFolderId);
}
