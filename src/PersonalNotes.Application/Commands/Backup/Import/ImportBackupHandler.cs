using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Options;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Backup.Import
{
    public class ImportBackupHandler : IRequestHandler<ImportBackupCommand, ImportBackupResponse>
    {
        private readonly IFolderRepository _folders;
        private readonly IDocumentRepository _documents;
        private readonly INotesOptions _options;
        private readonly ILogger<ImportBackupHandler> _logger;

        public ImportBackupHandler(IFolderRepository folders, IDocumentRepository documents, INotesOptions options,
            ILogger<ImportBackupHandler> logger)
        {
            _folders = folders;
            _documents = documents;
            _options = options;
            _logger = logger;
        }

        public async Task<ImportBackupResponse> Handle(ImportBackupCommand command, CancellationToken ct)
        {
            var folders = command.Backup.Folders.Select(f => f.ToEntity()).ToList();
            var documents = command.Backup.Documents.Select(d => d.ToEntity()).ToList();

            // Todo se valida antes de borrar: un respaldo inválido no deja la base a medias
            ValidateFolders(folders);
            ValidateDocuments(documents, folders);

            await _documents.ReplaceAllAsync(documents, ct);
            await _folders.ReplaceAllAsync(folders, ct);

            _logger.LogInformation("Respaldo importado: {Folders} carpeta(s) y {Documents} documento(s).",
                folders.Count, documents.Count);
            return new ImportBackupResponse(folders.Count, documents.Count);
        }

        private void ValidateFolders(IReadOnlyCollection<Folder> folders)
        {
            if (folders.Select(f => f.Id).Distinct().Count() != folders.Count)
                throw new BusinessRuleException("El respaldo tiene carpetas con el Id repetido.");

            var active = folders.Where(f => f.Enabled).ToList();
            var activeIds = active.Select(f => f.Id).ToHashSet();

            var orphan = active.FirstOrDefault(f => f.ParentFolderId is not null && !activeIds.Contains(f.ParentFolderId));
            if (orphan is not null)
                throw new BusinessRuleException($"La carpeta \"{orphan.Name}\" apunta a una carpeta padre inexistente o eliminada.");

            var duplicated = active
                .GroupBy(f => (f.ParentFolderId, f.NormalizedName))
                .FirstOrDefault(g => g.Count() > 1);
            if (duplicated is not null)
                throw new BusinessRuleException($"El respaldo tiene dos carpetas \"{duplicated.First().Name}\" en la misma ubicación.");

            // Con todos los padres presentes, un camino a la raíz más corto que la cadena real indica un ciclo
            var hierarchy = new FolderHierarchy(active);
            foreach (var folder in active)
            {
                var path = hierarchy.PathOf(folder.Id);
                if (path[0].ParentFolderId is not null)
                    throw new BusinessRuleException($"La carpeta \"{folder.Name}\" forma un ciclo en el árbol.");
                if (path.Count > _options.MaxFolderDepth)
                    throw new BusinessRuleException(
                        $"La carpeta \"{folder.Name}\" supera los {_options.MaxFolderDepth} niveles permitidos.");
            }
        }

        private static void ValidateDocuments(IReadOnlyCollection<Document> documents, IReadOnlyCollection<Folder> folders)
        {
            if (documents.Select(d => d.Id).Distinct().Count() != documents.Count)
                throw new BusinessRuleException("El respaldo tiene documentos con el Id repetido.");

            var folderIds = folders.ToDictionary(f => f.Id, f => f.Enabled);
            var orphan = documents.FirstOrDefault(d =>
                !folderIds.TryGetValue(d.FolderId, out var folderEnabled) || (d.Enabled && !folderEnabled));

            if (orphan is not null)
                throw new BusinessRuleException($"El documento \"{orphan.Title}\" apunta a una carpeta inexistente o eliminada.");
        }
    }
}
