using MediatR;

namespace PersonalNotes.Application.Queries.Folders.Get
{
    public record GetFoldersQuery : IRequest<List<FolderResponse>>;

    /// <param name="ParentFolderId">null si es una carpeta raíz.</param>
    /// <param name="Depth">Nivel en el árbol: 1 para una raíz.</param>
    /// <param name="Path">Nombres desde la raíz hasta esta carpeta (incluida), para mostrar la ruta.</param>
    /// <param name="DocumentCount">Documentos directamente en la carpeta (sin contar subcarpetas).</param>
    public record FolderResponse(
        string Id,
        string Name,
        string? ParentFolderId,
        int Depth,
        List<string> Path,
        int DocumentCount,
        DateTime CreatedOn,
        DateTime UpdatedOn);
}
