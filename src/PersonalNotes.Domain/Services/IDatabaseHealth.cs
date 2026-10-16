namespace PersonalNotes.Domain.Services
{
    public interface IDatabaseHealth
    {
        /// <summary>true si la base de datos responde dentro del tiempo indicado.</summary>
        Task<bool> IsAvailableAsync(TimeSpan timeout, CancellationToken ct = default);
    }
}
