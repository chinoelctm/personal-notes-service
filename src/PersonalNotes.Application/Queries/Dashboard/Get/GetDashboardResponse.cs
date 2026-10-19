namespace PersonalNotes.Application.Queries.Dashboard.Get
{
    /// <param name="TotalDocuments">Documentos en todo el árbol.</param>
    /// <param name="TotalFolders">Total de carpetas.</param>
    /// <param name="ByPriority">Cantidad de documentos por prioridad.</param>
    /// <param name="RecentlyEdited">Documentos editados dentro de la ventana de <paramref name="RecentWindowHours"/>.</param>
    /// <param name="LastEditedOn">Última edición de cualquier documento (UTC); null si no hay documentos.</param>
    public record GetDashboardResponse(
        int TotalDocuments,
        int TotalFolders,
        PriorityCounts ByPriority,
        int RecentlyEdited,
        int RecentWindowHours,
        DateTime? LastEditedOn);

    public record PriorityCounts(int High, int Medium, int Low, int None);
}
