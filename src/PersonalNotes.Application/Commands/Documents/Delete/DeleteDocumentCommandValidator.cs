using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Commands.Documents.Delete
{
    public class DeleteDocumentCommandValidator : AbstractValidator<DeleteDocumentCommand>
    {
        public DeleteDocumentCommandValidator()
        {
            RuleFor(x => x.Id).MustBeObjectId("El Id del documento");
        }
    }
}
