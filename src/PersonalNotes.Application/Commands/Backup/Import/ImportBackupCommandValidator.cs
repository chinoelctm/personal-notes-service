using FluentValidation;
using PersonalNotes.Application.Commands.Documents;
using PersonalNotes.Application.Commands.Folders;
using PersonalNotes.Application.DTOs;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Backup.Import
{
    public class ImportBackupCommandValidator : AbstractValidator<ImportBackupCommand>
    {
        public ImportBackupCommandValidator()
        {
            RuleFor(x => x.Backup)
                .NotNull().WithMessage("El respaldo es obligatorio.");

            When(x => x.Backup is not null, () =>
            {
                RuleFor(x => x.Backup.Version)
                    .InclusiveBetween(1, BackupDto.CurrentVersion)
                    .WithMessage("La versión del respaldo no es compatible.");

                RuleFor(x => x.Backup.Folders)
                    .NotNull().WithMessage("El respaldo debe incluir la lista de carpetas.");
                RuleFor(x => x.Backup.Documents)
                    .NotNull().WithMessage("El respaldo debe incluir la lista de documentos.");

                RuleForEach(x => x.Backup.Folders).ChildRules(folder =>
                {
                    folder.RuleFor(f => f.Id).MustBeObjectId("El Id de la carpeta");
                    folder.RuleFor(f => f.Name).MustBeValidFolderName();
                    folder.RuleFor(f => f.ParentFolderId).MustBeOptionalObjectId("El Id de la carpeta padre");
                });

                RuleForEach(x => x.Backup.Documents).ChildRules(document =>
                {
                    document.RuleFor(d => d.Id).MustBeObjectId("El Id del documento");
                    document.RuleFor(d => d.FolderId).MustBeObjectId("El Id de la carpeta");
                    document.RuleFor(d => d.Title).MustBeValidTitle();
                    document.RuleFor(d => d.Content).MustBeValidContent();
                    document.RuleFor(d => d.Priority).MustBeValidPriority();
                    document.RuleFor(d => d.Type).IsInEnum().WithMessage("El tipo debe ser Rich o Html.");
                });
            });
        }
    }
}
