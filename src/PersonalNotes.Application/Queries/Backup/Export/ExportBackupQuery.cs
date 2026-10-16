using MediatR;
using PersonalNotes.Application.DTOs;

namespace PersonalNotes.Application.Queries.Backup.Export
{
    public record ExportBackupQuery : IRequest<BackupDto>;
}
