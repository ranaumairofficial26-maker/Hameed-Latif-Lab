using System.Collections.Concurrent;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;

namespace HammadLatifLab.ChatBot.Infrastructure.Repositories;

public class InMemoryChatLogRepository : IChatLogRepository
{
    private readonly ConcurrentBag<ChatLogEntry> _logs = new();

    public Task AddAsync(ChatLogEntry entry, CancellationToken ct = default)
    {
        _logs.Add(entry);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<ChatLogEntry>> GetRecentAsync(int count = 50, CancellationToken ct = default)
    {
        var recent = _logs
            .OrderByDescending(l => l.Timestamp)
            .Take(count)
            .ToList();

        return Task.FromResult<IReadOnlyList<ChatLogEntry>>(recent);
    }
}
