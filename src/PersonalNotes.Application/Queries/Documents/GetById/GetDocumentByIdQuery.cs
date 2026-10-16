using MediatR;

namespace PersonalNotes.Application.Queries.Documents.GetById
{
    public record GetDocumentByIdQuery(string Id) : IRequest<DocumentResponse>;
}
