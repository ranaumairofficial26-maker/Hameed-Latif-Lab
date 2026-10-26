namespace HammadLatifLab.ChatBot.Application.Interfaces;

public interface IChatSessionStore
{
    Task<IReadOnlyList<(string Role, string Content)>> GetHistoryAsync(string sessionId, CancellationToken ct = default);
    Task AddMessageAsync(string sessionId, string role, string content, CancellationToken ct = default);
}
