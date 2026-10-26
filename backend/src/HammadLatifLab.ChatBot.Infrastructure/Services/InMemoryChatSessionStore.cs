using System.Collections.Concurrent;
using HammadLatifLab.ChatBot.Application.Interfaces;

namespace HammadLatifLab.ChatBot.Infrastructure.Services;

public class InMemoryChatSessionStore : IChatSessionStore
{
    private class SessionRecord
    {
        public List<(string Role, string Content)> Messages { get; } = new();
        public DateTimeOffset LastAccessed { get; set; } = DateTimeOffset.UtcNow;
        public object Lock { get; } = new();
    }

    private readonly ConcurrentDictionary<string, SessionRecord> _sessions = new();
    private readonly TimeSpan _expiry;

    public InMemoryChatSessionStore(TimeSpan? expiry = null)
    {
        _expiry = expiry ?? TimeSpan.FromMinutes(30);
    }

    public Task<IReadOnlyList<(string Role, string Content)>> GetHistoryAsync(string sessionId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Task.FromResult<IReadOnlyList<(string Role, string Content)>>(Array.Empty<(string, string)>());
        }

        if (_sessions.TryGetValue(sessionId, out var session))
        {
            lock (session.Lock)
            {
                if (DateTimeOffset.UtcNow - session.LastAccessed > _expiry)
                {
                    _sessions.TryRemove(sessionId, out _);
                    return Task.FromResult<IReadOnlyList<(string Role, string Content)>>(Array.Empty<(string, string)>());
                }

                session.LastAccessed = DateTimeOffset.UtcNow;
                return Task.FromResult<IReadOnlyList<(string Role, string Content)>>(session.Messages.ToList().AsReadOnly());
            }
        }

        return Task.FromResult<IReadOnlyList<(string Role, string Content)>>(Array.Empty<(string, string)>());
    }

    public Task AddMessageAsync(string sessionId, string role, string content, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Task.CompletedTask;
        }

        var session = _sessions.GetOrAdd(sessionId, _ => new SessionRecord());

        lock (session.Lock)
        {
            if (DateTimeOffset.UtcNow - session.LastAccessed > _expiry)
            {
                session.Messages.Clear();
            }

            session.LastAccessed = DateTimeOffset.UtcNow;
            session.Messages.Add((role, content));

            // Keep only the last 6 messages
            while (session.Messages.Count > 6)
            {
                session.Messages.RemoveAt(0);
            }
        }

        return Task.CompletedTask;
    }
}
