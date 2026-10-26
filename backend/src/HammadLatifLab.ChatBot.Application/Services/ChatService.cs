using HammadLatifLab.ChatBot.Application.Common;
using HammadLatifLab.ChatBot.Application.DTOs;
using HammadLatifLab.ChatBot.Application.Interfaces;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HammadLatifLab.ChatBot.Application.Services;

public class ChatService : IChatService
{
    private readonly IFaqRepository _faqRepository;
    private readonly ITestRepository _testRepository;
    private readonly IChatLogRepository _chatLogRepository;
    private readonly IChatSessionStore _sessionStore;
    private readonly IGeminiClient _geminiClient;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IFaqRepository faqRepository,
        ITestRepository testRepository,
        IChatLogRepository chatLogRepository,
        IChatSessionStore sessionStore,
        IGeminiClient geminiClient,
        ILogger<ChatService> logger)
    {
        _faqRepository = faqRepository;
        _testRepository = testRepository;
        _chatLogRepository = chatLogRepository;
        _sessionStore = sessionStore;
        _geminiClient = geminiClient;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        var sessionId = string.IsNullOrWhiteSpace(request.SessionId) 
            ? Guid.NewGuid().ToString("N") 
            : request.SessionId;

        // 1. Load conversation history
        var history = await _sessionStore.GetHistoryAsync(sessionId, cancellationToken);
        var isGreeting = ResponseSanitizer.IsGreeting(request.Message);

        // 2. Fetch Lab Overview and All Test Names (capped at 300)
        var allTests = await _testRepository.GetAllAsync(cancellationToken);
        var totalTestsCount = allTests.Count;
        var allTestNames = allTests.Select(t => t.Name).Take(300).ToList();

        // 3. Search for Matched Tests (top 5) and Matched FAQs (top 3)
        var matchedTests = (await _testRepository.SearchAsync(request.Message, top: 5, cancellationToken)).ToList();
        var matchedFaqs = (await _faqRepository.SearchAsync(request.Message, top: 3, cancellationToken)).ToList();

        var matchedTestIds = matchedTests.Select(t => t.Id).ToList();
        var matchedFaqIds = matchedFaqs.Select(f => f.Id).ToList();

        _logger.LogInformation(
            "Session {SessionId}: Query matched {TestCount} tests and {FaqCount} FAQs. HistoryLength={HistoryLength}. Calling Gemini...",
            sessionId,
            matchedTests.Count,
            matchedFaqs.Count,
            history.Count);

        // 4. Build Prompt with structured context and history
        var prompt = PromptBuilder.BuildPrompt(
            request.Message,
            totalTestsCount,
            allTestNames,
            matchedTests,
            matchedFaqs,
            history);

        string finalAnswer;
        bool isGrounded;

        // 5. ALWAYS call Gemini for every valid message
        try
        {
            var geminiResponse = await _geminiClient.GenerateContentAsync(prompt, cancellationToken);
            if (string.IsNullOrWhiteSpace(geminiResponse))
            {
                _logger.LogWarning("Gemini returned empty text for session {SessionId}. Returning fallback message.", sessionId);
                finalAnswer = PromptBuilder.GetFallbackMessage(request.Message);
                isGrounded = false;
            }
            else
            {
                var trimmed = geminiResponse.Trim();
                finalAnswer = ResponseSanitizer.Clean(trimmed, isGreeting);
                bool greetingStripped = !string.Equals(finalAnswer, trimmed, StringComparison.Ordinal);

                _logger.LogInformation(
                    "Session {SessionId}: HistoryLength={HistoryLength}, GreetingStripped={GreetingStripped}, GeminiCalled=True",
                    sessionId,
                    history.Count,
                    greetingStripped);

                isGrounded = true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini client call failed or timed out for session {SessionId}. Returning fallback message.", sessionId);
            finalAnswer = PromptBuilder.GetFallbackMessage(request.Message);
            isGrounded = false;
        }

        // 6. Save messages to session store (last 6 retained automatically)
        await _sessionStore.AddMessageAsync(sessionId, "User", request.Message, cancellationToken);
        await _sessionStore.AddMessageAsync(sessionId, "Assistant", finalAnswer, cancellationToken);

        // 7. Log interaction asynchronously
        var logEntry = new ChatLogEntry
        {
            SessionId = sessionId,
            UserQuestion = request.Message,
            AssistantAnswer = finalAnswer,
            SourceFaqIds = matchedFaqIds,
            FallbackTriggered = !isGrounded,
            Timestamp = DateTimeOffset.UtcNow
        };
        await _chatLogRepository.AddAsync(logEntry, cancellationToken);

        return new ChatResponse
        {
            Answer = finalAnswer,
            IsGrounded = isGrounded,
            MatchedFaqIds = matchedFaqIds,
            MatchedTestIds = matchedTestIds,
            SessionId = sessionId
        };
    }
}
