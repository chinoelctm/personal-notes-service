using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Queries.Documents;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Queries.Folders.GetDocuments
{
    public class GetFolderDocumentsHandler : IRequestHandler<GetFolderDocumentsQuery, List<DocumentResponse>>
    {
        private readonly IFolderRepository _folders;
        private readonly IDocumentRepository _documents;
        private readonly ILogger<GetFolderDocumentsHandler> _logger;

        public GetFolderDocumentsHandler(IFolderRepository folders, IDocumentRepository documents,
            ILogger<GetFolderDocumentsHandler> logger)
        {
            _folders = folders;
            _documents = documents;
            _logger = logger;
        }

        public async Task<List<DocumentResponse>> Handle(GetFolderDocumentsQuery query, CancellationToken ct)
        {
            var folder = await _folders.GetByIdAsync(query.FolderId, ct);
            if (folder is null)
            {
                _logger.LogError("No existe la carpeta {FolderId}.", query.FolderId);
                throw new NotFoundException("La carpeta no existe.");
            }

            var documents = await _documents.GetByFolderAsync(query.FolderId, ct);
            return documents.Select(DocumentResponse.From).ToList();
        }
    }
}
