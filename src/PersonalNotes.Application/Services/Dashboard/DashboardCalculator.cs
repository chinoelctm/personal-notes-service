using PersonalNotes.Application.Queries.Dashboard.Get;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Application.Services.Dashboard
{
    /// <summary>Estadísticas de las tarjetas del dashboard: totales, desglose por prioridad y actividad reciente.</summary>
    public class DashboardCalculator : IDashboardCalculator
    {
        public GetDashboardResponse Calculate(IReadOnlyCollection<DocumentStamp> documents, int totalFolders,
            DateTime utcNow, int recentWindowHours)
        {
            var byPriority = documents
                .GroupBy(d => d.Priority)
                .ToDictionary(g => g.Key, g => g.Count());

            // Ventana cerrada en ambos extremos: una nota editada justo hace 24 h todavía cuenta como reciente
            var since = utcNow.AddHours(-recentWindowHours);
            var recent = documents.Count(d => d.UpdatedOn >= since && d.UpdatedOn <= utcNow);

            return new GetDashboardResponse(
                documents.Count,
                totalFolders,
                new PriorityCounts(
                    byPriority.GetValueOrDefault(Priority.High),
                    byPriority.GetValueOrDefault(Priority.Medium),
                    byPriority.GetValueOrDefault(Priority.Low),
                    byPriority.GetValueOrDefault(Priority.None)),
                recent,
                recentWindowHours,
                documents.Count == 0 ? null : documents.Max(d => d.UpdatedOn));
        }
    }
}
