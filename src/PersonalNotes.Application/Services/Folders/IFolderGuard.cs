namespace PersonalNotes.Application.Services.Folders
{
    public interface IFolderGuard
    {
        /// <summary>Lanza NotFoundException si la carpeta no existe o fue eliminada.</summary>
        Task EnsureActiveAsync(string folderId, CancellationToken ct = default);
    }
}
