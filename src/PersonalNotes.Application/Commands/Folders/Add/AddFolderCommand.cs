using MediatR;

namespace PersonalNotes.Application.Commands.Folders.Add
{
    /// <param name="ParentFolderId">Carpeta contenedora; null o vacío para una carpeta raíz.</param>
    public record AddFolderCommand(string Name, string? ParentFolderId) : IRequest<string>;
}
