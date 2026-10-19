using PersonalNotes.Domain.ValueObjects;

namespace PersonalNotes.Domain.Services
{
    /// <summary>Proveedor de IA generativa (hoy Google Gemini). La API Key vive solo en la configuración del servidor.</summary>
    public interface IAiAssistant
    {
        /// <summary>true si hay API Key configurada.</summary>
        bool IsConfigured { get; }

        /// <summary>Modelo configurado (ej. gemini-3.8-flash).</summary>
        string Model { get; }

        /// <summary>Responde <paramref name="prompt"/> teniendo en cuenta la conversación previa.</summary>
        Task<string> AskAsync(IReadOnlyList<ChatTurn> history, string prompt, CancellationToken ct = default);
    }
}
