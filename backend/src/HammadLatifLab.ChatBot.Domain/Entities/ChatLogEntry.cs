namespace HammadLatifLab.ChatBot.Domain.Entities;

/// <summary>
/// Audit and diagnostic record of a conversation turn.
/// </summary>
public sealed class ChatLogEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string SessionId { get; init; } = string.Empty;
    public string UserQuestion { get; init; } = string.Empty;
    public string AssistantAnswer { get; init; } = string.Empty;
    public IReadOnlyList<int> SourceFaqIds { get; init; } = Array.Empty<int>();
    public bool FallbackTriggered { get; init; }
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
