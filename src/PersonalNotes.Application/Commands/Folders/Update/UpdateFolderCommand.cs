using MediatR;

namespace PersonalNotes.Application.Commands.Folders.Update
{
    /// <summary>Renombra la carpeta y/o la mueve a otra carpeta padre (null = raíz), con todo su contenido.</summary>
    public record UpdateFolderCommand(string Id, string Name, string? ParentFolderId) : IRequest;
}
