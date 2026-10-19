using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Queries.Documents.GetById
{
    public class GetDocumentByIdHandler : IRequestHandler<GetDocumentByIdQuery, DocumentResponse>
    {
        private readonly IDocumentRepository _repository;
        private readonly ILogger<GetDocumentByIdHandler> _logger;

        public GetDocumentByIdHandler(IDocumentRepository repository, ILogger<GetDocumentByIdHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task<DocumentResponse> Handle(GetDocumentByIdQuery query, CancellationToken ct)
        {
            var document = await _repository.GetByIdAsync(query.Id, ct);
            if (document is null)
            {
                _logger.LogError("No existe el documento {DocumentId}.", query.Id);
                throw new NotFoundException("El documento no existe.");
            }

            return DocumentResponse.From(document);
        }
    }
}
