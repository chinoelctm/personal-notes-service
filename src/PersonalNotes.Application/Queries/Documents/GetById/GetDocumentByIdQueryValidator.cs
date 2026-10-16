using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Queries.Documents.GetById
{
    public class GetDocumentByIdQueryValidator : AbstractValidator<GetDocumentByIdQuery>
    {
        public GetDocumentByIdQueryValidator()
        {
            RuleFor(x => x.Id).MustBeObjectId("El Id del documento");
        }
    }
}
