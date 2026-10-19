using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Documents.Update
{
    public class UpdateDocumentHandler : IRequestHandler<UpdateDocumentCommand, UpdateDocumentResponse>
    {
        private readonly IDocumentRepository _repository;
        private readonly IFolderGuard _folderGuard;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateDocumentHandler> _logger;

        public UpdateDocumentHandler(IDocumentRepository repository, IFolderGuard folderGuard, TimeHelper timeHelper,
            ILogger<UpdateDocumentHandler> logger)
        {
            _repository = repository;
            _folderGuard = folderGuard;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<UpdateDocumentResponse> Handle(UpdateDocumentCommand command, CancellationToken ct)
        {
            var document = await _repository.GetByIdAsync(command.Id, ct);
            if (document is null)
            {
                _logger.LogError("No existe el documento {DocumentId}.", command.Id);
                throw new NotFoundException("El documento no existe.");
            }

            if (document.FolderId != command.FolderId)
                await _folderGuard.EnsureExistsAsync(command.FolderId, ct);

            document.Update(command.FolderId, command.Title, command.Content, command.Priority, _timeHelper.GetUtcNow(),
                command.Type);
            await _repository.UpdateAsync(document, ct);

            _logger.LogInformation("Documento {DocumentId} ({Title}) publicado; prioridad {Priority}.",
                document.Id, document.Title, document.Priority);
            return new UpdateDocumentResponse(document.Id, document.UpdatedOn);
        }
    }
}
