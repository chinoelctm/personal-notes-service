using PersonalNotes.Infrastructure.Context;

namespace PersonalNotes.Api.Workers
{
    /// <summary>
    /// Crea los índices de MongoDB en segundo plano. Si la base no responde al arrancar (por ejemplo,
    /// Atlas bloquea la IP o aún no está disponible), la API sigue levantada y se reintenta con
    /// espera creciente en vez de detener el proceso.
    /// </summary>
    public class MongoIndexInitializer : BackgroundService
    {
        private static readonly TimeSpan InitialDelay = TimeSpan.FromSeconds(5);
        private static readonly TimeSpan MaxDelay = TimeSpan.FromMinutes(5);

        private readonly PersonalNotesDbContext _context;
        private readonly ILogger<MongoIndexInitializer> _logger;

        public MongoIndexInitializer(PersonalNotesDbContext context, ILogger<MongoIndexInitializer> logger)
        {
            _context = context;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // No retrasar el arranque del servidor HTTP
            await Task.Yield();

            var delay = InitialDelay;
            for (var attempt = 1; !stoppingToken.IsCancellationRequested; attempt++)
            {
                try
                {
                    await _context.EnsureIndexesAsync(stoppingToken);
                    _logger.LogInformation("Índices de MongoDB verificados (intento {Attempt}).", attempt);
                    return;
                }
                catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
                {
                    return;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "No se pudieron crear los índices de MongoDB (intento {Attempt}). Reintento en {Seconds}s.",
                        attempt, delay.TotalSeconds);
                }

                try
                {
                    await Task.Delay(delay, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    return;
                }

                delay = TimeSpan.FromTicks(Math.Min(delay.Ticks * 2, MaxDelay.Ticks));
            }
        }
    }
}
