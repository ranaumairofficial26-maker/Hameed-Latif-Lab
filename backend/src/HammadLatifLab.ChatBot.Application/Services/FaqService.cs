using HammadLatifLab.ChatBot.Application.DTOs;
using HammadLatifLab.ChatBot.Application.Interfaces;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;

namespace HammadLatifLab.ChatBot.Application.Services;

public class FaqService : IFaqService
{
    private readonly IFaqRepository _faqRepository;

    public FaqService(IFaqRepository faqRepository)
    {
        _faqRepository = faqRepository;
    }

    public async Task<IReadOnlyList<FaqDto>> GetAllFaqsAsync(CancellationToken ct = default)
    {
        var items = await _faqRepository.GetAllAsync(ct);
        return items.Select(MapToDto).ToList();
    }

    public async Task<FaqDto?> GetFaqByIdAsync(int id, CancellationToken ct = default)
    {
        var item = await _faqRepository.GetByIdAsync(id, ct);
        return item is null ? null : MapToDto(item);
    }

    public async Task<IReadOnlyList<FaqDto>> SearchFaqsAsync(string query, int top = 5, CancellationToken ct = default)
    {
        var items = await _faqRepository.SearchAsync(query, top, ct);
        return items.Select(MapToDto).ToList();
    }

    private static FaqDto MapToDto(FaqItem item) => new()
    {
        Id = item.Id,
        Category = item.Category,
        Question = item.Question,
        QuestionUrdu = item.QuestionUrdu,
        Answer = item.Answer,
        Keywords = item.Keywords
    };
}
