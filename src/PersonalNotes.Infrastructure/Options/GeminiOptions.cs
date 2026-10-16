using System.ComponentModel.DataAnnotations;

namespace PersonalNotes.Infrastructure.Options
{
    /// <summary>Sección "Gemini" de appsettings.json. Sin ApiKey la API arranca igual y el asistente responde 503.</summary>
    public class GeminiOptions
    {
        public const string SectionName = "Gemini";

        // Se obtiene gratis en https://aistudio.google.com/app/apikey. No subirla al repo con un valor real
        public string ApiKey { get; set; } = string.Empty;

        [Required]
        public string Model { get; set; } = "gemini-2.5-flash";

        [Required, Url]
        public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";

        [Range(5, 300)]
        public int TimeoutSeconds { get; set; } = 60;
    }
}
