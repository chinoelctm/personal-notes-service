using MongoDB.Bson;
using MongoDB.Driver;
using PersonalNotes.Domain.Services;
using PersonalNotes.Infrastructure.Context;

namespace PersonalNotes.Infrastructure.Services
{
    public class MongoDatabaseHealth : IDatabaseHealth
    {
        private readonly IMongoDatabase _database;

        public MongoDatabaseHealth(PersonalNotesDbContext context)
        {
            _database = context.Database;
        }

        public async Task<bool> IsAvailableAsync(TimeSpan timeout, CancellationToken ct = default)
        {
            // Sin este límite, un ping a una base caída esperaría el timeout completo del driver
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
            cts.CancelAfter(timeout);

            try
            {
                await _database
                    .RunCommandAsync<BsonDocument>(new BsonDocument("ping", 1), cancellationToken: cts.Token)
                    .ConfigureAwait(false);
                return true;
            }
            catch (Exception ex) when (ex is MongoException or TimeoutException or OperationCanceledException
                                       && !ct.IsCancellationRequested)
            {
                return false;
            }
        }
    }
}
