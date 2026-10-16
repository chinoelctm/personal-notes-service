using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Entities;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Options;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Folders.Add
{
    public class AddFolderHandler : IRequestHandler<AddFolderCommand, string>
    {
        private readonly IFolderRepository _repository;
        private readonly IFolderHierarchyProvider _hierarchyProvider;
        private readonly INotesOptions _options;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<AddFolderHandler> _logger;

        public AddFolderHandler(IFolderRepository repository, IFolderHierarchyProvider hierarchyProvider,
            INotesOptions options, TimeHelper timeHelper, ILogger<AddFolderHandler> logger)
        {
            _repository = repository;
            _hierarchyProvider = hierarchyProvider;
            _options = options;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task<string> Handle(AddFolderCommand command, CancellationToken ct)
        {
            var folder = Folder.Create(command.Name, command.ParentFolderId, _timeHelper.GetUtcNow());

            var hierarchy = await _hierarchyProvider.GetAsync(ct);
            hierarchy.EnsureCanPlace(folderId: null, folder.ParentFolderId, _options.MaxFolderDepth);

            if (await _repository.ExistsActiveByNameAsync(folder.ParentFolderId, folder.Name, ct: ct))
                throw new ConflictException("Ya existe una carpeta con ese nombre en la misma ubicación.");

            await _repository.AddAsync(folder, ct);

            _logger.LogInformation("Carpeta {FolderId} ({Name}) creada en {ParentFolderId}.",
                folder.Id, folder.Name, folder.ParentFolderId ?? "la raíz");
            return folder.Id;
        }
    }
}
