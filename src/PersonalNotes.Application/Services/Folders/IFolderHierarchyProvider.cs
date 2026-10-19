namespace PersonalNotes.Application.Services.Folders
{
    public interface IFolderHierarchyProvider
    {
        /// <summary>Árbol con todas las carpetas.</summary>
        Task<FolderHierarchy> GetAsync(CancellationToken ct = default);
    }
}
