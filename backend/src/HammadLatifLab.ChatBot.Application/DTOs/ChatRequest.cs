namespace HammadLatifLab.ChatBot.Application.DTOs;

public class ChatRequest
{
    public string Message { get; set; } = string.Empty;
    public string? SessionId { get; set; }
}
