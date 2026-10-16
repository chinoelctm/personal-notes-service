using MediatR;
using PersonalNotes.Domain.Services;

namespace PersonalNotes.Application.Queries.Assistant.GetStatus
{
    public class GetAssistantStatusHandler : IRequestHandler<GetAssistantStatusQuery, GetAssistantStatusResponse>
    {
        private readonly IAiAssistant _assistant;

        public GetAssistantStatusHandler(IAiAssistant assistant)
        {
            _assistant = assistant;
        }

        public Task<GetAssistantStatusResponse> Handle(GetAssistantStatusQuery query, CancellationToken ct)
        {
            return Task.FromResult(new GetAssistantStatusResponse(_assistant.IsConfigured, "Google Gemini", _assistant.Model));
        }
    }
}
