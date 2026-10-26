using HammadLatifLab.ChatBot.Application.Common;
using HammadLatifLab.ChatBot.Application.DTOs;
using HammadLatifLab.ChatBot.Application.Interfaces;
using HammadLatifLab.ChatBot.Application.Services;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HammadLatifLab.ChatBot.Tests.Services;

public class ChatServiceTests
{
    private readonly Mock<IFaqRepository> _faqRepoMock = new();
    private readonly Mock<ITestRepository> _testRepoMock = new();
    private readonly Mock<IChatLogRepository> _chatLogRepoMock = new();
    private readonly Mock<IChatSessionStore> _sessionStoreMock = new();
    private readonly Mock<IGeminiClient> _geminiClientMock = new();

    public ChatServiceTests()
    {
        // Default empty search returns
        _faqRepoMock
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<FaqItem>());

        _testRepoMock
            .Setup(r => r.SearchAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestItem>());

        // Default empty session history
        _sessionStoreMock
            .Setup(s => s.GetHistoryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<(string, string)>());

        // Default test catalog (e.g., 752 tests)
        var dummyTests = Enumerable.Range(1, 752).Select(i => new TestItem
        {
            Id = i,
            Name = $"Test #{i}",
            Price = 1000,
            Time = "After 24 Hours",
            Keywords = new List<string> { $"test{i}" }
        }).ToList();

        _testRepoMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(dummyTests);
    }

    [Fact]
    public async Task ProcessMessageAsync_GeneralQuestionNoMatch_GeminiIsCalledAndReplyReturned_FallbackNotUsed()
    {
        // Requirement 6: General question, no match: Gemini IS called and its reply is returned (fallback NOT used).
        _geminiClientMock
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("We offer over 752 diagnostic tests covering hematology, biochemistry, and microbiology.");

        var service = new ChatService(
            _faqRepoMock.Object,
            _testRepoMock.Object,
            _chatLogRepoMock.Object,
            _sessionStoreMock.Object,
            _geminiClientMock.Object,
            NullLogger<ChatService>.Instance);

        var request = new ChatRequest { Message = "can u please tell me about the test" };
        var response = await service.ProcessMessageAsync(request);

        Assert.True(response.IsGrounded);
        Assert.Equal("We offer over 752 diagnostic tests covering hematology, biochemistry, and microbiology.", response.Answer);
        Assert.DoesNotContain("+92-42-111-455-522", response.Answer);
        _geminiClientMock.Verify(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_WhenGeminiThrowsOrReturnsEmpty_ReturnsFallbackMessage()
    {
        // Requirement 6: Gemini throws or returns empty: fallback message returned.
        _geminiClientMock
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Gemini API Timeout"));

        var service = new ChatService(
            _faqRepoMock.Object,
            _testRepoMock.Object,
            _chatLogRepoMock.Object,
            _sessionStoreMock.Object,
            _geminiClientMock.Object,
            NullLogger<ChatService>.Instance);

        var request = new ChatRequest { Message = "how many test you have in your database" };
        var response = await service.ProcessMessageAsync(request);

        Assert.False(response.IsGrounded);
        Assert.Contains("+92-42-111-455-522", response.Answer);
        _geminiClientMock.Verify(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessMessageAsync_ContextContainsCorrectTotalTestCount()
    {
        // Requirement 6: Context contains the correct total test count.
        string? capturedPrompt = null;
        _geminiClientMock
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((p, _) => capturedPrompt = p)
            .ReturnsAsync("We have 752 tests available.");

        var service = new ChatService(
            _faqRepoMock.Object,
            _testRepoMock.Object,
            _chatLogRepoMock.Object,
            _sessionStoreMock.Object,
            _geminiClientMock.Object,
            NullLogger<ChatService>.Instance);

        var request = new ChatRequest { Message = "how many test you have in your database" };
        await service.ProcessMessageAsync(request);

        Assert.NotNull(capturedPrompt);
        Assert.Contains("Total tests available: 752", capturedPrompt);
    }

    [Fact]
    public async Task ProcessMessageAsync_MatchedTestLineContainsExactPriceAndTimeFromRepository()
    {
        // Requirement 6: Matched test line contains the exact price and time from the repository.
        var matchedTest = new TestItem
        {
            Id = 154,
            Name = "Blood Complete Examination (CBC)",
            Price = 900,
            Time = "After 24 Hours",
            Keywords = new List<string> { "cbc" }
        };

        _testRepoMock
            .Setup(r => r.SearchAsync("cbc price", It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<TestItem> { matchedTest });

        string? capturedPrompt = null;
        _geminiClientMock
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((p, _) => capturedPrompt = p)
            .ReturnsAsync("Test: Blood Complete Examination (CBC) | Report time: After 24 Hours | Price: Rs. 900");

        var service = new ChatService(
            _faqRepoMock.Object,
            _testRepoMock.Object,
            _chatLogRepoMock.Object,
            _sessionStoreMock.Object,
            _geminiClientMock.Object,
            NullLogger<ChatService>.Instance);

        var request = new ChatRequest { Message = "cbc price" };
        var response = await service.ProcessMessageAsync(request);

        Assert.NotNull(capturedPrompt);
        Assert.Contains("Test: Blood Complete Examination (CBC) | Report time: After 24 Hours | Price: Rs. 900", capturedPrompt);
        Assert.Contains("900", response.Answer);
    }

    [Fact]
    public async Task ProcessMessageAsync_WithNonEmptyHistory_PromptContainsDoNotGreetDirective()
    {
        // Requirement 6: ChatService: with non-empty history the prompt contains the "Do NOT greet or welcome the user again" line.
        var history = new List<(string Role, string Content)>
        {
            ("User", "hello"),
            ("Assistant", "How can I help you today?")
        };

        _sessionStoreMock
            .Setup(s => s.GetHistoryAsync("session-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(history);

        string? capturedPrompt = null;
        _geminiClientMock
            .Setup(g => g.GenerateContentAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Callback<string, CancellationToken>((p, _) => capturedPrompt = p)
            .ReturnsAsync("We offer CBC, Sugar, and Lipid profiles.");

        var service = new ChatService(
            _faqRepoMock.Object,
            _testRepoMock.Object,
            _chatLogRepoMock.Object,
            _sessionStoreMock.Object,
            _geminiClientMock.Object,
            NullLogger<ChatService>.Instance);

        var request = new ChatRequest { Message = "what tests do you have", SessionId = "session-123" };
        await service.ProcessMessageAsync(request);

        Assert.NotNull(capturedPrompt);
        Assert.Contains("Do NOT greet or welcome the user again", capturedPrompt);
        Assert.Contains("=== CONVERSATION SO FAR ===", capturedPrompt);
        Assert.Contains("User: hello", capturedPrompt);
    }
}
