using MediatR;
using PersonalNotes.Domain.Services;

namespace PersonalNotes.Application.Queries.Health
{
    public class GetHealthHandler : IRequestHandler<GetHealthQuery, GetHealthResponse>
    {
        private static readonly TimeSpan PingTimeout = TimeSpan.FromSeconds(5);

        private readonly IDatabaseHealth _databaseHealth;

        public GetHealthHandler(IDatabaseHealth databaseHealth)
        {
            _databaseHealth = databaseHealth;
        }

        public async Task<GetHealthResponse> Handle(GetHealthQuery query, CancellationToken ct)
        {
            var databaseUp = await _databaseHealth.IsAvailableAsync(PingTimeout, ct);
            return databaseUp
                ? new GetHealthResponse("Healthy", "Up")
                : new GetHealthResponse("Degraded", "Down");
        }
    }
}
