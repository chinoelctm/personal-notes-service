using PersonalNotes.Application.Queries.Dashboard.Get;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Services.Dashboard
{
    public interface IDashboardCalculator
    {
        GetDashboardResponse Calculate(IReadOnlyCollection<DocumentStamp> documents, int totalFolders, DateTime utcNow,
            int recentWindowHours);
    }
}
