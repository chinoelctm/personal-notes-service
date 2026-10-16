using MediatR;
using PersonalNotes.Application.DTOs;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Queries.Backup.Export
{
    public class ExportBackupHandler : IRequestHandler<ExportBackupQuery, BackupDto>
    {
        private readonly IFolderRepository _folders;
        private readonly IDocumentRepository _documents;
        private readonly TimeHelper _timeHelper;

        public ExportBackupHandler(IFolderRepository folders, IDocumentRepository documents, TimeHelper timeHelper)
        {
            _folders = folders;
            _documents = documents;
            _timeHelper = timeHelper;
        }

        public async Task<BackupDto> Handle(ExportBackupQuery query, CancellationToken ct)
        {
            var folders = await _folders.GetAllAsync(ct);
            var documents = await _documents.GetAllAsync(ct);

            return new BackupDto(
                BackupDto.CurrentVersion,
                _timeHelper.GetUtcNow(),
                folders.Select(FolderBackupDto.From).ToList(),
                documents.Select(DocumentBackupDto.From).ToList());
        }
    }
}
