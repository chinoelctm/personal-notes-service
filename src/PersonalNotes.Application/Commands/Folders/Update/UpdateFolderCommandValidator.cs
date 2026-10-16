using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Folders.Update
{
    public class UpdateFolderCommandValidator : AbstractValidator<UpdateFolderCommand>
    {
        public UpdateFolderCommandValidator()
        {
            RuleFor(x => x.Id).MustBeObjectId("El Id de la carpeta");
            RuleFor(x => x.Name).MustBeValidFolderName();
            RuleFor(x => x.ParentFolderId).MustBeOptionalObjectId("El Id de la carpeta padre");
        }
    }
}
