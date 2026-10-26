namespace HammadLatifLab.ChatBot.Application.Interfaces;

public interface IGeminiClient
{
    Task<string> GenerateContentAsync(string prompt, CancellationToken cancellationToken = default);
}
