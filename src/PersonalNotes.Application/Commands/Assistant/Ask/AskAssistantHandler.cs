using MediatR;
using Microsoft.Extensions.Logging;
using PersonalNotes.Application.Services.Assistant;
using PersonalNotes.Domain.Services;

namespace PersonalNotes.Application.Commands.Assistant.Ask
{
    public class AskAssistantHandler : IRequestHandler<AskAssistantCommand, AskAssistantResponse>
    {
        private readonly IAiAssistant _assistant;
        private readonly AssistantPromptBuilder _promptBuilder;
        private readonly ILogger<AskAssistantHandler> _logger;

        public AskAssistantHandler(IAiAssistant assistant, AssistantPromptBuilder promptBuilder,
            ILogger<AskAssistantHandler> logger)
        {
            _assistant = assistant;
            _promptBuilder = promptBuilder;
            _logger = logger;
        }

        public async Task<AskAssistantResponse> Handle(AskAssistantCommand command, CancellationToken ct)
        {
            var prompt = _promptBuilder.BuildPrompt(command.Question, command.Context);
            var history = _promptBuilder.BuildHistory(command.History);

            // No se registra el contenido de la nota ni la pregunta: solo su tamaño
            _logger.LogInformation("Consulta al asistente ({Model}): {Chars} caracteres, {History} mensaje(s) previos, contexto: {Context}.",
                _assistant.Model, prompt.Length, history.Count, command.Context is not null);

            var answer = await _assistant.AskAsync(history, prompt, ct);
            return new AskAssistantResponse(answer, _assistant.Model);
        }
    }
}
