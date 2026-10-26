using HammadLatifLab.ChatBot.Infrastructure.Services;
using Xunit;

namespace HammadLatifLab.ChatBot.Tests.Services;

public class InMemoryChatSessionStoreTests
{
    [Fact]
    public async Task AddMessageAsync_KeepsOnlyLast6Messages()
    {
        // Requirement 6: Session store: keeps only the last 6 messages
        var store = new InMemoryChatSessionStore(TimeSpan.FromMinutes(30));
        var sessionId = "test-session-1";

        for (int i = 1; i <= 10; i++)
        {
            await store.AddMessageAsync(sessionId, "User", $"Message {i}");
        }

        var history = await store.GetHistoryAsync(sessionId);

        Assert.Equal(6, history.Count);
        Assert.Equal("Message 5", history[0].Content);
        Assert.Equal("Message 10", history[5].Content);
    }

    [Fact]
    public async Task GetHistoryAsync_ExpiresAfterInactivityPeriod()
    {
        // Requirement 6: Session store: expires after 30 minutes
        // We initialize with a tiny TimeSpan (50ms) to test expiration behavior
        var store = new InMemoryChatSessionStore(TimeSpan.FromMilliseconds(50));
        var sessionId = "expiring-session";

        await store.AddMessageAsync(sessionId, "User", "Hello there");
        var activeHistory = await store.GetHistoryAsync(sessionId);
        Assert.Single(activeHistory);

        // Wait past expiry
        await Task.Delay(100);

        var expiredHistory = await store.GetHistoryAsync(sessionId);
        Assert.Empty(expiredHistory);
    }
}
