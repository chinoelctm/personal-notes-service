namespace PersonalNotes.Api.Models.Folder.AddFolder
{
    /// <param name="Name">Nombre de la carpeta (único entre sus hermanas activas).</param>
    /// <param name="ParentFolderId">Carpeta contenedora; null para crearla en la raíz.</param>
    public record AddFolderRequest(
        string Name,
        string? ParentFolderId);
}
