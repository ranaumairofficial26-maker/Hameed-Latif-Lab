namespace HammadLatifLab.ChatBot.Application.DTOs;

public class FaqDto
{
    public int Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Question { get; set; } = string.Empty;
    public string? QuestionUrdu { get; set; }
    public string Answer { get; set; } = string.Empty;
    public IReadOnlyList<string> Keywords { get; set; } = Array.Empty<string>();
}
