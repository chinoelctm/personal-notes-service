using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Documents.Add
{
    public class AddDocumentCommandValidator : AbstractValidator<AddDocumentCommand>
    {
        public AddDocumentCommandValidator()
        {
            RuleFor(x => x.FolderId).MustBeObjectId("El Id de la carpeta");
            RuleFor(x => x.Title).MustBeValidTitle();
            RuleFor(x => x.Content).MustBeValidContent();
            RuleFor(x => x.Priority).MustBeValidPriority();
        }
    }
}
