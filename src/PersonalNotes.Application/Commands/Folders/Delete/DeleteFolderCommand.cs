using MediatR;

namespace PersonalNotes.Application.Commands.Folders.Delete
{
    /// <summary>
    /// Borra de forma permanente la carpeta, todas sus subcarpetas y los documentos que contienen (borrado físico en cascada).
    /// </summary>
    public record DeleteFolderCommand(string Id) : IRequest;
}
