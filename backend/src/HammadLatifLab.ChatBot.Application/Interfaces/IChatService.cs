using HammadLatifLab.ChatBot.Application.DTOs;

namespace HammadLatifLab.ChatBot.Application.Interfaces;

public interface IChatService
{
    Task<ChatResponse> ProcessMessageAsync(ChatRequest request, CancellationToken cancellationToken = default);
}
