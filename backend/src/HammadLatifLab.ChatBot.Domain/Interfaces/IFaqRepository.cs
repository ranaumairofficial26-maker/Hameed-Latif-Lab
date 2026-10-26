using HammadLatifLab.ChatBot.Domain.Entities;

namespace HammadLatifLab.ChatBot.Domain.Interfaces;

/// <summary>
/// Contract for retrieving and searching verified FAQs.
/// </summary>
public interface IFaqRepository
{
    Task<IReadOnlyList<FaqItem>> GetAllAsync(CancellationToken ct = default);
    Task<IReadOnlyList<FaqItem>> SearchAsync(string query, int top = 3, CancellationToken ct = default);
    Task<FaqItem?> GetByIdAsync(int id, CancellationToken ct = default);
}
