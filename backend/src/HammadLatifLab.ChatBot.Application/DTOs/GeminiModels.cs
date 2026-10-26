using System.Text.Json.Serialization;

namespace HammadLatifLab.ChatBot.Application.DTOs;

public class GeminiApiRequest
{
    [JsonPropertyName("contents")]
    public List<GeminiContent> Contents { get; set; } = new();

    [JsonPropertyName("generationConfig")]
    public GeminiGenerationConfig? GenerationConfig { get; set; }
}

public class GeminiContent
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("parts")]
    public List<GeminiPart> Parts { get; set; } = new();
}

public class GeminiPart
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}

public class GeminiGenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 0.2;

    [JsonPropertyName("maxOutputTokens")]
    public int MaxOutputTokens { get; set; } = 512;
}

public class GeminiApiResponse
{
    [JsonPropertyName("candidates")]
    public List<GeminiCandidate>? Candidates { get; set; }
}

public class GeminiCandidate
{
    [JsonPropertyName("content")]
    public GeminiContent? Content { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }
}
