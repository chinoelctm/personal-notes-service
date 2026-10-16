using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Options;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Commands.Folders.Update
{
    public class UpdateFolderHandler : IRequestHandler<UpdateFolderCommand>
    {
        private readonly IFolderRepository _repository;
        private readonly IFolderHierarchyProvider _hierarchyProvider;
        private readonly INotesOptions _options;
        private readonly TimeHelper _timeHelper;
        private readonly ILogger<UpdateFolderHandler> _logger;

        public UpdateFolderHandler(IFolderRepository repository, IFolderHierarchyProvider hierarchyProvider,
            INotesOptions options, TimeHelper timeHelper, ILogger<UpdateFolderHandler> logger)
        {
            _repository = repository;
            _hierarchyProvider = hierarchyProvider;
            _options = options;
            _timeHelper = timeHelper;
            _logger = logger;
        }

        public async Task Handle(UpdateFolderCommand command, CancellationToken ct)
        {
            var folder = await _repository.GetByIdAsync(command.Id, ct);
            if (folder is null || !folder.Enabled)
            {
                _logger.LogError("No existe la carpeta {FolderId}.", command.Id);
                throw new NotFoundException("La carpeta no existe.");
            }

            var previousParent = folder.ParentFolderId;
            folder.Update(command.Name, command.ParentFolderId, _timeHelper.GetUtcNow());

            // Solo un movimiento puede romper la profundidad o crear un ciclo
            if (folder.ParentFolderId != previousParent)
            {
                var hierarchy = await _hierarchyProvider.GetAsync(ct);
                hierarchy.EnsureCanPlace(folder.Id, folder.ParentFolderId, _options.MaxFolderDepth);
            }

            if (await _repository.ExistsActiveByNameAsync(folder.ParentFolderId, folder.Name, excludeId: folder.Id, ct))
                throw new ConflictException("Ya existe una carpeta con ese nombre en la misma ubicación.");

            await _repository.UpdateAsync(folder, ct);

            _logger.LogInformation("Carpeta {FolderId} actualizada: {Name} en {ParentFolderId}.",
                folder.Id, folder.Name, folder.ParentFolderId ?? "la raíz");
        }
    }
}
