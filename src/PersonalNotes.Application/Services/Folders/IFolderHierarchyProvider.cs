namespace PersonalNotes.Application.Services.Folders
{
    public interface IFolderHierarchyProvider
    {
        /// <summary>Árbol con las carpetas activas.</summary>
        Task<FolderHierarchy> GetAsync(CancellationToken ct = default);
    }
}
