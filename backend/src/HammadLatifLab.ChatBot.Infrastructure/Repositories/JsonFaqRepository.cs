using System.Text.Json;
using System.Text.RegularExpressions;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HammadLatifLab.ChatBot.Infrastructure.Repositories;

public class JsonFaqRepository : IFaqRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonFaqRepository> _logger;
    private readonly List<FaqItem> _faqs = new();
    private readonly object _lock = new();
    private bool _isLoaded;

    public static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "can", "u", "you", "please", "tell", "me", "about", "the", "a", "an", "is", "are",
        "do", "does", "have", "how", "many", "what", "which", "your", "in", "of", "for",
        "to", "test", "tests", "kya", "hai", "hain", "ka", "ki", "ke", "mein", "me",
        "batao", "bataye", "kitna", "kitne", "rate", "price", "paisay", "report", "kab"
    };

    public JsonFaqRepository(string filePath, ILogger<JsonFaqRepository> logger)
    {
        _filePath = filePath;
        _logger = logger;
    }

    private void EnsureLoaded()
    {
        if (_isLoaded) return;

        lock (_lock)
        {
            if (_isLoaded) return;

            try
            {
                if (File.Exists(_filePath))
                {
                    var json = File.ReadAllText(_filePath);
                    var items = JsonSerializer.Deserialize<List<FaqItem>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (items != null)
                    {
                        _faqs.Clear();
                        _faqs.AddRange(items);
                        _logger.LogInformation("Loaded {Count} FAQs from {Path}", _faqs.Count, _filePath);
                    }
                }
                else
                {
                    _logger.LogWarning("FAQ JSON file not found at {Path}", _filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading FAQ JSON file at {Path}", _filePath);
            }
            finally
            {
                _isLoaded = true;
            }
        }
    }

    public Task<IReadOnlyList<FaqItem>> GetAllAsync(CancellationToken ct = default)
    {
        EnsureLoaded();
        return Task.FromResult<IReadOnlyList<FaqItem>>(_faqs.AsReadOnly());
    }

    public Task<FaqItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        EnsureLoaded();
        var item = _faqs.FirstOrDefault(f => f.Id == id);
        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<FaqItem>> SearchAsync(string query, int top = 3, CancellationToken ct = default)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<FaqItem>>(Array.Empty<FaqItem>());
        }

        var tokens = Regex.Split(query, @"[^a-zA-Z0-9]+")
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length >= 2 && !Stopwords.Contains(t))
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<FaqItem>>(Array.Empty<FaqItem>());
        }

        var scored = new List<(FaqItem Item, int Score)>();

        foreach (var faq in _faqs)
        {
            int score = 0;
            var questionEn = faq.Question.ToLowerInvariant();
            var questionUr = (faq.QuestionUrdu ?? string.Empty).ToLowerInvariant();
            var answer = faq.Answer.ToLowerInvariant();
            var category = faq.Category.ToLowerInvariant();

            foreach (var token in tokens)
            {
                var termRegex = new Regex($@"\b{Regex.Escape(token)}\b", RegexOptions.IgnoreCase);

                // Question match weighs more
                if (termRegex.IsMatch(questionEn))
                {
                    score += 20;
                }
                else if (questionEn.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                }

                if (!string.IsNullOrWhiteSpace(questionUr))
                {
                    if (termRegex.IsMatch(questionUr))
                    {
                        score += 20;
                    }
                    else if (questionUr.Contains(token, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 10;
                    }
                }

                // Keyword match
                foreach (var kw in faq.Keywords)
                {
                    var kwLower = kw.ToLowerInvariant();
                    if (string.Equals(kwLower, token, StringComparison.OrdinalIgnoreCase) || termRegex.IsMatch(kwLower))
                    {
                        score += 15;
                    }
                    else if (kwLower.Contains(token, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 8;
                    }
                }

                // Category match
                if (termRegex.IsMatch(category)) score += 10;

                // Answer match
                if (termRegex.IsMatch(answer)) score += 5;
            }

            if (score > 0)
            {
                scored.Add((faq, score));
            }
        }

        var results = scored
            .OrderByDescending(s => s.Score)
            .Take(top)
            .Select(s => s.Item)
            .ToList();

        return Task.FromResult<IReadOnlyList<FaqItem>>(results.AsReadOnly());
    }
}
