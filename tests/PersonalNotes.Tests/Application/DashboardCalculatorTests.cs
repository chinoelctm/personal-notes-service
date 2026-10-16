using PersonalNotes.Application.Services.Dashboard;
using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Tests.Application
{
    public class DashboardCalculatorTests
    {
        private static readonly DateTime Now = new(2026, 10, 16, 12, 0, 0, DateTimeKind.Utc);
        private static readonly string FolderId = ObjectIds.New();
        private readonly DashboardCalculator _calculator = new();

        private static DocumentStamp Stamp(Priority priority, DateTime updatedOn) =>
            new(ObjectIds.New(), FolderId, priority, updatedOn);

        [Fact]
        public void Calculate_CountsTotalsAndPriorities()
        {
            var stamps = new[]
            {
                Stamp(Priority.High, Now.AddHours(-6)),
                Stamp(Priority.High, Now.AddDays(-3)),
                Stamp(Priority.Medium, Now.AddHours(-9)),
                Stamp(Priority.Low, Now.AddHours(-14)),
                Stamp(Priority.None, Now.AddDays(-10))
            };

            var result = _calculator.Calculate(stamps, totalFolders: 4, Now, recentWindowHours: 24);

            Assert.Equal((5, 4), (result.TotalDocuments, result.TotalFolders));
            Assert.Equal((2, 1, 1, 1),
                (result.ByPriority.High, result.ByPriority.Medium, result.ByPriority.Low, result.ByPriority.None));
            Assert.Equal(3, result.RecentlyEdited);
            Assert.Equal(Now.AddHours(-6), result.LastEditedOn);
        }

        [Fact]
        public void Calculate_RecentWindowIncludesItsBoundary()
        {
            var stamps = new[]
            {
                Stamp(Priority.None, Now.AddHours(-24)),
                Stamp(Priority.None, Now.AddHours(-24).AddTicks(-1))
            };

            var result = _calculator.Calculate(stamps, totalFolders: 1, Now, recentWindowHours: 24);

            Assert.Equal(1, result.RecentlyEdited);
        }

        [Fact]
        public void Calculate_IgnoresFutureDatesInRecentCount()
        {
            var result = _calculator.Calculate([Stamp(Priority.Low, Now.AddMinutes(5))], 1, Now, 24);

            Assert.Equal(0, result.RecentlyEdited);
        }

        [Fact]
        public void Calculate_WithoutDocuments_ReturnsZeros()
        {
            var result = _calculator.Calculate([], totalFolders: 0, Now, recentWindowHours: 24);

            Assert.Equal((0, 0, 0, (DateTime?)null), (result.TotalDocuments, result.ByPriority.High,
                result.RecentlyEdited, result.LastEditedOn));
        }
    }
}
