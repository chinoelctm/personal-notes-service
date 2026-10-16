using MediatR;

namespace PersonalNotes.Application.Commands.Folders.Delete
{
    /// <summary>
    /// Desactiva la carpeta, todas sus subcarpetas y los documentos que contienen (borrado lógico en cascada).
    /// </summary>
    public record DeleteFolderCommand(string Id) : IRequest;
}
