using FluentValidation;
using MediatR;
using PersonalNotes.Application.Behaviors;
using PersonalNotes.Application.Commands.Documents.Add;
using PersonalNotes.Application.Services.Assistant;
using PersonalNotes.Application.Services.Dashboard;
using PersonalNotes.Application.Services.Folders;

namespace PersonalNotes.Api.Extensions
{
    public static class ApplicationExtensions
    {
        public static IServiceCollection AddApplication(
            this IServiceCollection services)
        {
            services.AddMediatR(cfg =>
                cfg.RegisterServicesFromAssembly(typeof(AddDocumentCommand).Assembly));

            services.AddValidatorsFromAssembly(
                typeof(AddDocumentCommandValidator).Assembly);

            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
            services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

            services.AddScoped<IFolderHierarchyProvider, FolderHierarchyProvider>();
            services.AddScoped<IFolderGuard, FolderGuard>();
            services.AddSingleton<IDashboardCalculator, DashboardCalculator>();
            services.AddSingleton<AssistantPromptBuilder>();
            services.AddSingleton<NotesKnowledgeBuilder>();

            return services;
        }
    }
}
