using MediatR;
using PersonalNotes.Application.DTOs;

namespace PersonalNotes.Application.Commands.Backup.Import
{
    /// <summary>Reemplaza todas las carpetas y documentos por los del respaldo.</summary>
    public record ImportBackupCommand(BackupDto Backup) : IRequest<ImportBackupResponse>;

    public record ImportBackupResponse(int Folders, int Documents);
}
