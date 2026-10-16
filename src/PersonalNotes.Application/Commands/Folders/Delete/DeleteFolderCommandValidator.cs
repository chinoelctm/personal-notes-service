using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Folders.Delete
{
    public class DeleteFolderCommandValidator : AbstractValidator<DeleteFolderCommand>
    {
        public DeleteFolderCommandValidator()
        {
            RuleFor(x => x.Id).MustBeObjectId("El Id de la carpeta");
        }
    }
}
