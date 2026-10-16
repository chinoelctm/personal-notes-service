using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Documents.Update
{
    public class UpdateDocumentCommandValidator : AbstractValidator<UpdateDocumentCommand>
    {
        public UpdateDocumentCommandValidator()
        {
            RuleFor(x => x.Id).MustBeObjectId("El Id del documento");
            RuleFor(x => x.FolderId).MustBeObjectId("El Id de la carpeta");
            RuleFor(x => x.Title).MustBeValidTitle();
            RuleFor(x => x.Content).MustBeValidContent();
            RuleFor(x => x.Priority).MustBeValidPriority();
        }
    }
}
