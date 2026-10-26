using System.Net.Http.Json;
using System.Text.Json;
using HammadLatifLab.ChatBot.Application.Configuration;
using HammadLatifLab.ChatBot.Application.DTOs;
using HammadLatifLab.ChatBot.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace HammadLatifLab.ChatBot.Infrastructure.Clients;

public class GeminiClient : IGeminiClient
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiClient> _logger;

    public GeminiClient(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        ILogger<GeminiClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("Gemini API key is not configured. Returning fallback.");
            throw new InvalidOperationException("Gemini API key is not configured.");
        }

        var endpoint = $"models/{_options.Model}:generateContent";

        var requestPayload = new GeminiApiRequest
        {
            Contents = new List<GeminiContent>
            {
                new()
                {
                    Role = "user",
                    Parts = new List<GeminiPart>
                    {
                        new() { Text = prompt }
                    }
                }
            },
            GenerationConfig = new GeminiGenerationConfig
            {
                Temperature = _options.Temperature,
                MaxOutputTokens = _options.MaxOutputTokens
            }
        };

        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, endpoint);
        requestMessage.Headers.Add("x-goog-api-key", _options.ApiKey);
        requestMessage.Content = JsonContent.Create(requestPayload);

        var response = await _httpClient.SendAsync(requestMessage, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogError("Gemini API error: Status {StatusCode} ({Reason})", response.StatusCode, response.ReasonPhrase);
            throw new HttpRequestException($"Gemini API returned status code {response.StatusCode}.");
        }

        var responseData = await response.Content.ReadFromJsonAsync<GeminiApiResponse>(cancellationToken: cancellationToken);
        var reply = responseData?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;

        return reply ?? string.Empty;
    }
}
