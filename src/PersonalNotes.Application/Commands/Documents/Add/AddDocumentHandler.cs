using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Documents.Add
{
    public class AddDocumentHandler : IRequestHandler<AddDocumentCommand, AddDocumentResponse>
    {
        private readonly IDocumentRepository _repository;
        private readonly IFolderGuard _folderGuard;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<AddDocumentHandler> _logger;

        public AddDocumentHandler(IDocumentRepository repository, IFolderGuard folderGuard, TimeHelper timeHelper,
            ILogger<AddDocumentHandler> logger)
        {
            _repository = repository;
            _folderGuard = folderGuard;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<AddDocumentResponse> Handle(AddDocumentCommand command, CancellationToken ct)
        {
            await _folderGuard.EnsureActiveAsync(command.FolderId, ct);

            var document = Document.Create(command.FolderId, command.Title, command.Content, command.Priority,
                _timeHelper.GetUtcNow());
            await _repository.AddAsync(document, ct);

            _logger.LogInformation("Documento {DocumentId} ({Title}) publicado en la carpeta {FolderId}.",
                document.Id, document.Title, document.FolderId);
            return new AddDocumentResponse(document.Id, document.UpdatedOn);
        }
    }
}
