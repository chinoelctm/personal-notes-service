namespace PersonalNotes.Domain.ValueObjects
{
    public enum ChatRole
    {
        User,
        Assistant
    }

    /// <summary>Un mensaje previo de la conversación con el asistente.</summary>
    public record ChatTurn(ChatRole Role, string Text);
}
