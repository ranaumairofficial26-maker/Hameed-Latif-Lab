namespace HammadLatifLab.ChatBot.Application.Configuration;

public class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gemini-3.5-flash-lite";
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta";
    public double Temperature { get; set; } = 0.2;
    public int MaxOutputTokens { get; set; } = 512;
}
