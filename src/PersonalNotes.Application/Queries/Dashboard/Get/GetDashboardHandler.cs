using MediatR;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Dashboard;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Options;
using PersonalNotes.Domain.Repositories;

namespace PersonalNotes.Application.Queries.Dashboard.Get
{
    public class GetDashboardHandler : IRequestHandler<GetDashboardQuery, GetDashboardResponse>
    {
        private readonly IDocumentRepository _documents;
        private readonly IFolderHierarchyProvider _hierarchyProvider;
        private readonly IDashboardCalculator _calculator;
        private readonly INotesOptions _options;
        private readonly TimeHelper _timeHelper;

        public GetDashboardHandler(IDocumentRepository documents, IFolderHierarchyProvider hierarchyProvider,
            IDashboardCalculator calculator, INotesOptions options, TimeHelper timeHelper)
        {
            _documents = documents;
            _hierarchyProvider = hierarchyProvider;
            _calculator = calculator;
            _options = options;
            _timeHelper = timeHelper;
        }

        public async Task<GetDashboardResponse> Handle(GetDashboardQuery query, CancellationToken ct)
        {
            var hierarchy = await _hierarchyProvider.GetAsync(ct);
            var stamps = await _documents.GetStampsAsync(ct);

            // Un documento cuya carpeta ya no existe no se ve en el árbol: tampoco se cuenta
            var visible = stamps.Where(s => hierarchy.Contains(s.FolderId)).ToList();

            return _calculator.Calculate(visible, hierarchy.Folders.Count, _timeHelper.GetUtcNow(),
                _options.RecentWindowHours);
        }
    }
}
