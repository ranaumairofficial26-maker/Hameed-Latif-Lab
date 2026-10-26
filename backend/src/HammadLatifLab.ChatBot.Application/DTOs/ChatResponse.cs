namespace HammadLatifLab.ChatBot.Application.DTOs;

public class ChatResponse
{
    public string Answer { get; set; } = string.Empty;
    public bool IsGrounded { get; set; }
    public List<int> MatchedFaqIds { get; set; } = new();
    public List<int> MatchedTestIds { get; set; } = new();
    public string SessionId { get; set; } = string.Empty;
}
