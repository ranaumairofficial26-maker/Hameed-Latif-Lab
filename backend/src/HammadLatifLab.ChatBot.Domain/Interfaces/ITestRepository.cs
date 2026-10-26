using HammadLatifLab.ChatBot.Domain.Entities;

namespace HammadLatifLab.ChatBot.Domain.Interfaces;

/// <summary>
/// Contract for retrieving and searching laboratory tests and price list.
/// </summary>
public interface ITestRepository
{
    Task<IReadOnlyList<TestItem>> GetAllAsync(CancellationToken ct = default);
    Task<TestItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<TestItem>> SearchAsync(string query, int top = 5, CancellationToken ct = default);
}
