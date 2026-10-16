using FluentValidation;
using PersonalNotes.Domain.Entities;

namespace PersonalNotes.Application.Commands.Folders
{
    public static class FolderRules
    {
        public static IRuleBuilderOptions<T, string> MustBeValidFolderName<T>(this IRuleBuilder<T, string> rule)
        {
            return rule
                .NotEmpty().WithMessage("El nombre de la carpeta es obligatorio.")
                .Must(name => name is null || name.Trim().Length <= Folder.MaxNameLength)
                .WithMessage($"El nombre no puede superar los {Folder.MaxNameLength} caracteres.");
        }
    }
}
