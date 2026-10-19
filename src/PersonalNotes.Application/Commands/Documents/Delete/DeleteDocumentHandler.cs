using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Documents.Delete
{
    public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand>
    {
        private readonly IDocumentRepository _repository;
        private readonly ILogger<DeleteDocumentHandler> _logger;

        public DeleteDocumentHandler(IDocumentRepository repository, ILogger<DeleteDocumentHandler> logger)
        {
            _repository = repository;
            _logger = logger;
        }

        public async Task Handle(DeleteDocumentCommand command, CancellationToken ct)
        {
            if (!await _repository.DeleteAsync(command.Id, ct))
            {
                _logger.LogError("No existe el documento {DocumentId}.", command.Id);
                throw new NotFoundException("El documento no existe.");
            }

            _logger.LogInformation("Documento {DocumentId} borrado.", command.Id);
        }
    }
}
