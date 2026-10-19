using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Folders.Delete
{
    public class DeleteFolderHandler : IRequestHandler<DeleteFolderCommand>
    {
        private readonly IFolderRepository _folders;
        private readonly IDocumentRepository _documents;
        private readonly IFolderHierarchyProvider _hierarchyProvider;
        private readonly ILogger<DeleteFolderHandler> _logger;

        public DeleteFolderHandler(IFolderRepository folders, IDocumentRepository documents,
            IFolderHierarchyProvider hierarchyProvider, ILogger<DeleteFolderHandler> logger)
        {
            _folders = folders;
            _documents = documents;
            _hierarchyProvider = hierarchyProvider;
            _logger = logger;
        }

        public async Task Handle(DeleteFolderCommand command, CancellationToken ct)
        {
            var hierarchy = await _hierarchyProvider.GetAsync(ct);
            if (!hierarchy.Contains(command.Id))
            {
                _logger.LogError("No existe la carpeta {FolderId}.", command.Id);
                throw new NotFoundException("La carpeta no existe.");
            }

            var subtree = hierarchy.SubtreeOf(command.Id);

            // Primero los documentos: si falla a mitad, las carpetas siguen visibles y se puede reintentar
            // sin dejar documentos huérfanos
            var documents = await _documents.DeleteByFoldersAsync(subtree, ct);
            var folders = await _folders.DeleteManyAsync(subtree, ct);

            _logger.LogInformation("Carpeta {FolderId} borrada: {Folders} carpeta(s) y {Documents} documento(s).",
                command.Id, folders, documents);
        }
    }
}
