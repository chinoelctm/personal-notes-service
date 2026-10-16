using Microsoft.Extensions.Options;
using MongoDB.Driver;
using PersonalNotes.Api.Workers;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Domain.Options;
using PersonalNotes.Domain.Repositories;
using PersonalNotes.Domain.Services;
using PersonalNotes.Infrastructure.Configurations;
using PersonalNotes.Infrastructure.Context;
using PersonalNotes.Infrastructure.Options;
using PersonalNotes.Infrastructure.Persistence;
using PersonalNotes.Infrastructure.Services;

namespace PersonalNotes.Api.Extensions
{
    public static class InfrastructureExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddMongoDb(configuration)
                .AddTime()
                .AddRepositories()
                .AddOptions(configuration)
                .AddAssistant(configuration);

            return services;
        }

        private static IServiceCollection AddMongoDb(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<MongoConfiguration>()
                .Bind(configuration.GetSection(MongoConfiguration.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<IMongoClient>(sp =>
            {
                var opts     = sp.GetRequiredService<IOptions<MongoConfiguration>>().Value;
                var settings = MongoClientSettings.FromConnectionString(opts.MongoUri);

                settings.MaxConnectionPoolSize  = opts.MaxConnectionPool;
                settings.MinConnectionPoolSize  = opts.MinConnectionPool;
                settings.MaxConnectionIdleTime  = TimeSpan.FromMinutes(opts.MaxIdleMinutes);
                settings.MaxConnectionLifeTime  = TimeSpan.FromMinutes(opts.MaxLifetimeMinutes);
                settings.RetryReads             = true;
                settings.RetryWrites            = true;
                settings.ConnectTimeout         = TimeSpan.FromMilliseconds(opts.TimeoutMs);
                settings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(opts.TimeoutMs);
                settings.SocketTimeout          = TimeSpan.FromMilliseconds(opts.TimeoutMs);
                settings.WaitQueueTimeout       = TimeSpan.FromMilliseconds(opts.TimeoutMs);

                return new MongoClient(settings);
            });

            services.AddSingleton(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                var opts   = sp.GetRequiredService<IOptions<MongoConfiguration>>().Value;
                return new PersonalNotesDbContext(client, opts.MongoDb);
            });

            return services;
        }

        // Las fechas de auditoría son UTC; el front las muestra en la hora local del navegador
        private static IServiceCollection AddTime(this IServiceCollection services)
        {
            services.AddSingleton<TimeHelper>();

            return services;
        }

        private static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IFolderRepository, FolderRepository>();
            services.AddScoped<IDocumentRepository, DocumentRepository>();
            services.AddSingleton<IDatabaseHealth, MongoDatabaseHealth>();

            // Índices en segundo plano: la API arranca aunque MongoDB todavía no responda
            services.AddHostedService<MongoIndexInitializer>();

            return services;
        }

        private static IServiceCollection AddOptions(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<NotesOptions>()
                .Bind(configuration.GetSection(NotesOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton<INotesOptions>(sp =>
                sp.GetRequiredService<IOptions<NotesOptions>>().Value);

            return services;
        }

        // Asistente de IA: la API Key solo vive en el servidor (Gemini:ApiKey); sin ella el endpoint responde 503
        private static IServiceCollection AddAssistant(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddOptions<GeminiOptions>()
                .Bind(configuration.GetSection(GeminiOptions.SectionName))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            services.AddSingleton(sp => sp.GetRequiredService<IOptions<GeminiOptions>>().Value);

            services.AddHttpClient<IAiAssistant, GeminiAssistant>((sp, client) =>
                client.Timeout = TimeSpan.FromSeconds(sp.GetRequiredService<GeminiOptions>().TimeoutSeconds));

            return services;
        }
    }
}
