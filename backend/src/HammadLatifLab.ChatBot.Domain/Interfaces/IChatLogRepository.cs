using HammadLatifLab.ChatBot.Domain.Entities;

namespace HammadLatifLab.ChatBot.Domain.Interfaces;

/// <summary>
/// Contract for recording chat interactions for audit and debugging purposes.
/// </summary>
public interface IChatLogRepository
{
    Task AddAsync(ChatLogEntry entry, CancellationToken ct = default);
    Task<IReadOnlyList<ChatLogEntry>> GetRecentAsync(int count = 50, CancellationToken ct = default);
}
