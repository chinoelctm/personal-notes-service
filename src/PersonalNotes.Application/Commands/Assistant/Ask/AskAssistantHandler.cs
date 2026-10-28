using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.DTOs;
using PersonalNotes.Application.Helpers;
using PersonalNotes.Application.Services.Assistant;
using PersonalNotes.Application.Services.Folders;
using PersonalNotes.Domain.Repositories;
using PersonalNotes.Domain.Services;

namespace PersonalNotes.Application.Commands.Assistant.Ask
{
    public class AskAssistantHandler : IRequestHandler<AskAssistantCommand, AskAssistantResponse>
    {
        private readonly IAiAssistant _assistant;
        private readonly IDocumentRepository _documents;
        private readonly IFolderHierarchyProvider _hierarchyProvider;
        private readonly AssistantPromptBuilder _promptBuilder;
        private readonly NotesKnowledgeBuilder _knowledgeBuilder;
        private readonly ILogger<AskAssistantHandler> _logger;

        public AskAssistantHandler(IAiAssistant assistant, IDocumentRepository documents,
            IFolderHierarchyProvider hierarchyProvider, AssistantPromptBuilder promptBuilder,
            NotesKnowledgeBuilder knowledgeBuilder, ILogger<AskAssistantHandler> logger)
        {
            _assistant = assistant;
            _documents = documents;
            _hierarchyProvider = hierarchyProvider;
            _promptBuilder = promptBuilder;
            _knowledgeBuilder = knowledgeBuilder;
            _logger = logger;
        }

        public async Task<AskAssistantResponse> Handle(AskAssistantCommand command, CancellationToken ct)
        {
            var knowledge = command.SearchNotes == false
                ? null
                : await BuildKnowledgeAsync(command.Question, command.Context?.DocumentId, ct);

            var prompt = _promptBuilder.BuildPrompt(command.Question, command.Context, knowledge);
            var history = _promptBuilder.BuildHistory(command.History);

            // No se registra el contenido de las notas ni la pregunta: solo tamaños
            _logger.LogInformation(
                "Consulta al asistente ({Model}): {Chars} caracteres, {History} mensaje(s) previos, contexto: {Context}, notas enviadas: {Sources}.",
                _assistant.Model, prompt.Length, history.Count, command.Context is not null, knowledge?.Sources.Count ?? 0);

            var answer = await _assistant.AskAsync(history, prompt, ct);
            return new AskAssistantResponse(answer, _assistant.Model, knowledge?.Sources ?? []);
        }

        private async Task<NotesKnowledge> BuildKnowledgeAsync(string question, string? openDocumentId, CancellationToken ct)
        {
            var hierarchy = await _hierarchyProvider.GetAsync(ct);
            var documents = await _documents.GetAllAsync(ct);

            var notes = documents
                .Where(d => hierarchy.Contains(d.FolderId))
                .Select(d => new NoteForAssistant(
                    d.Id,
                    d.Title,
                    string.Join(" / ", hierarchy.PathOf(d.FolderId).Select(f => f.Name)),
                    MarkdownText.ToPlainText(d.Content, d.Type),
                    d.Priority,
                    d.UpdatedOn))
                .ToList();

            return _knowledgeBuilder.Build(question, notes, excludeId: openDocumentId);
        }
    }
}
