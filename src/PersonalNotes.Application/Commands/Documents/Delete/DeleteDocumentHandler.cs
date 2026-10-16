using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Documents.Delete
{
    public class DeleteDocumentHandler : IRequestHandler<DeleteDocumentCommand>
    {
        private readonly IDocumentRepository _repository;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<DeleteDocumentHandler> _logger;

        public DeleteDocumentHandler(IDocumentRepository repository, TimeHelper timeHelper,
            ILogger<DeleteDocumentHandler> logger)
        {
            _repository = repository;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(DeleteDocumentCommand command, CancellationToken ct)
        {
            var document = await _repository.GetByIdAsync(command.Id, ct);
            if (document is null || !document.Enabled)
            {
                _logger.LogError("No existe el documento {DocumentId}.", command.Id);
                throw new NotFoundException("El documento no existe.");
            }

            document.Disable(_timeHelper.GetUtcNow());
            await _repository.UpdateAsync(document, ct);

            _logger.LogInformation("Documento {DocumentId} ({Title}) eliminado.", document.Id, document.Title);
        }
    }
}
