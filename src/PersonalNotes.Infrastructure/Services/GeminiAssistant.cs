using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using PersonalNotes.Domain.Exceptions;
using PersonalNotes.Domain.Services;
using PersonalNotes.Domain.ValueObjects;
using PersonalNotes.Infrastructure.Options;

namespace PersonalNotes.Infrastructure.Services
{
    /// <summary>
    /// Cliente de la API generateContent de Google Gemini. La API Key viaja en el header x-goog-api-key
    /// (no en la URL, para que no quede en logs) y nunca llega al navegador.
    /// </summary>
    public class GeminiAssistant : IAiAssistant
    {
        private const string Instructions =
            "Eres el asistente de un gestor de notas personales. Responde en español, de forma clara y breve, " +
            "usando listas cuando ayuden. Si se adjunta un documento, básate en él.";

        private readonly HttpClient _http;
        private readonly GeminiOptions _options;
        private readonly ILogger<GeminiAssistant> _logger;

        public GeminiAssistant(HttpClient http, GeminiOptions options, ILogger<GeminiAssistant> logger)
        {
            _http = http;
            _options = options;
            _logger = logger;
        }

        public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

        public string Model => _options.Model;

        public async Task<string> AskAsync(IReadOnlyList<ChatTurn> history, string prompt, CancellationToken ct = default)
        {
            if (!IsConfigured)
                throw new AssistantNotConfiguredException(
                    "El asistente de IA no está configurado: falta Gemini:ApiKey en la configuración del servidor.");

            var contents = history
                .Select(t => new GeminiContent(t.Role == ChatRole.User ? "user" : "model", [new GeminiPart(t.Text)]))
                .Append(new GeminiContent("user", [new GeminiPart(prompt)]))
                .ToList();
            var body = new GeminiRequest(new GeminiInstruction([new GeminiPart(Instructions)]), contents);

            using var request = new HttpRequestMessage(HttpMethod.Post,
                $"{_options.BaseUrl.TrimEnd('/')}/models/{Uri.EscapeDataString(_options.Model)}:generateContent")
            {
                Content = JsonContent.Create(body)
            };
            request.Headers.Add("x-goog-api-key", _options.ApiKey);

            HttpResponseMessage response;
            try
            {
                response = await _http.SendAsync(request, ct).ConfigureAwait(false);
            }
            catch (TaskCanceledException) when (!ct.IsCancellationRequested)
            {
                throw new AssistantUnavailableException("Gemini no respondió a tiempo. Intenta de nuevo.");
            }
            catch (HttpRequestException ex)
            {
                _logger.LogWarning(ex, "No se pudo conectar con Gemini.");
                throw new AssistantUnavailableException("No se pudo conectar con Google Gemini.");
            }

            using (response)
            {
                if (!response.IsSuccessStatusCode)
                    throw await ToExceptionAsync(response, ct).ConfigureAwait(false);

                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>(ct).ConfigureAwait(false);
                var text = string.Concat(result?.Candidates?.FirstOrDefault()?.Content?.Parts?.Select(p => p.Text) ?? []).Trim();
                return text.Length > 0 ? text : "El asistente no devolvió una respuesta. Intenta reformular la pregunta.";
            }
        }

        private async Task<Exception> ToExceptionAsync(HttpResponseMessage response, CancellationToken ct)
        {
            var detail = await response.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            _logger.LogWarning("Gemini respondió {Status}: {Detail}", (int)response.StatusCode,
                detail.Length > 500 ? detail[..500] : detail);

            return response.StatusCode switch
            {
                HttpStatusCode.TooManyRequests => new AssistantQuotaExceededException(
                    "Se alcanzó el límite gratuito de Gemini. Espera un momento e intenta de nuevo."),
                HttpStatusCode.BadRequest when detail.Contains("API key", StringComparison.OrdinalIgnoreCase) =>
                    new AssistantUnavailableException("La API Key de Gemini configurada en el servidor no es válida."),
                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden => new AssistantUnavailableException(
                    "Google rechazó la API Key de Gemini configurada en el servidor."),
                HttpStatusCode.NotFound => new AssistantUnavailableException(
                    $"El modelo \"{_options.Model}\" no existe en Gemini; revisa Gemini:Model."),
                _ => new AssistantUnavailableException($"Gemini respondió con error {(int)response.StatusCode}.")
            };
        }

        private record GeminiPart([property: JsonPropertyName("text")] string Text);

        private record GeminiContent(
            [property: JsonPropertyName("role")] string Role,
            [property: JsonPropertyName("parts")] List<GeminiPart> Parts);

        private record GeminiInstruction([property: JsonPropertyName("parts")] List<GeminiPart> Parts);

        private record GeminiRequest(
            [property: JsonPropertyName("systemInstruction")] GeminiInstruction SystemInstruction,
            [property: JsonPropertyName("contents")] List<GeminiContent> Contents);

        private record GeminiResponse([property: JsonPropertyName("candidates")] List<GeminiCandidate>? Candidates);

        private record GeminiCandidate([property: JsonPropertyName("content")] GeminiResponseContent? Content);

        private record GeminiResponseContent([property: JsonPropertyName("parts")] List<GeminiResponsePart>? Parts);

        private record GeminiResponsePart([property: JsonPropertyName("text")] string? Text);
    }
}
