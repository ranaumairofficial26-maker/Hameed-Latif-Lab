namespace HammadLatifLab.ChatBot.Domain.Entities;

/// <summary>
/// Represents a laboratory test with its official name, reporting time, price in PKR, and search keywords.
/// </summary>
public sealed class TestItem
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Time { get; init; } = string.Empty;
    public int Price { get; init; }
    public IReadOnlyList<string> Keywords { get; init; } = Array.Empty<string>();
}
