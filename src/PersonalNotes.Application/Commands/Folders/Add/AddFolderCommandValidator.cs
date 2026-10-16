using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Folders.Add
{
    public class AddFolderCommandValidator : AbstractValidator<AddFolderCommand>
    {
        public AddFolderCommandValidator()
        {
            RuleFor(x => x.Name).MustBeValidFolderName();
            RuleFor(x => x.ParentFolderId).MustBeOptionalObjectId("El Id de la carpeta padre");
        }
    }
}
