namespace HammadLatifLab.ChatBot.Domain.Entities;

/// <summary>
/// Represents a verified knowledge base entry for the diagnostic laboratory.
/// </summary>
public sealed class FaqItem
{
    public int Id { get; init; }
    public string Question { get; init; } = string.Empty;
    public string? QuestionUrdu { get; init; }
    public string Answer { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();
}
