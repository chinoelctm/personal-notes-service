using FluentValidation;
using PersonalNotes.Application.Helpers;

namespace PersonalNotes.Application.Queries.Folders.GetDocuments
{
    public class GetFolderDocumentsQueryValidator : AbstractValidator<GetFolderDocumentsQuery>
    {
        public GetFolderDocumentsQueryValidator()
        {
            RuleFor(x => x.FolderId).MustBeObjectId("El Id de la carpeta");
        }
    }
}
