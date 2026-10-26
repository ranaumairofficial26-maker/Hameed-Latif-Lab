using HammadLatifLab.ChatBot.Application.DTOs;

namespace HammadLatifLab.ChatBot.Application.Interfaces;

public interface IFaqService
{
    Task<IReadOnlyList<FaqDto>> GetAllFaqsAsync(CancellationToken ct = default);
    Task<FaqDto?> GetFaqByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<FaqDto>> SearchFaqsAsync(string query, int top = 5, CancellationToken ct = default);
}
