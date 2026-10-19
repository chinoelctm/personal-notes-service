namespace PersonalNotes.Application.Services.Folders
{
    public interface IFolderGuard
    {
        /// <summary>Lanza NotFoundException si la carpeta no existe.</summary>
        Task EnsureExistsAsync(string folderId, CancellationToken ct = default);
    }
}
