using System.Text.Json;
using System.Text.RegularExpressions;
using HammadLatifLab.ChatBot.Domain.Entities;
using HammadLatifLab.ChatBot.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace HammadLatifLab.ChatBot.Infrastructure.Repositories;

public class JsonTestRepository : ITestRepository
{
    private readonly string _filePath;
    private readonly ILogger<JsonTestRepository> _logger;
    private readonly List<TestItem> _tests = new();
    private readonly object _lock = new();
    private bool _isLoaded;

    public static readonly HashSet<string> Stopwords = new(StringComparer.OrdinalIgnoreCase)
    {
        "can", "u", "you", "please", "tell", "me", "about", "the", "a", "an", "is", "are",
        "do", "does", "have", "how", "many", "what", "which", "your", "in", "of", "for",
        "to", "test", "tests", "kya", "hai", "hain", "ka", "ki", "ke", "mein", "me",
        "batao", "bataye", "kitna", "kitne", "rate", "price", "paisay", "report", "kab"
    };

    public JsonTestRepository(string filePath, ILogger<JsonTestRepository> logger)
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
                    var items = JsonSerializer.Deserialize<List<TestItem>>(json, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (items != null)
                    {
                        _tests.Clear();
                        _tests.AddRange(items);
                        _logger.LogInformation("Loaded {Count} lab tests from {Path}", _tests.Count, _filePath);
                    }
                }
                else
                {
                    _logger.LogWarning("Tests JSON file not found at {Path}", _filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading Tests JSON file at {Path}", _filePath);
            }
            finally
            {
                _isLoaded = true;
            }
        }
    }

    public Task<IReadOnlyList<TestItem>> GetAllAsync(CancellationToken ct = default)
    {
        EnsureLoaded();
        return Task.FromResult<IReadOnlyList<TestItem>>(_tests.AsReadOnly());
    }

    public Task<TestItem?> GetByIdAsync(int id, CancellationToken ct = default)
    {
        EnsureLoaded();
        var item = _tests.FirstOrDefault(t => t.Id == id);
        return Task.FromResult(item);
    }

    public Task<IReadOnlyList<TestItem>> SearchAsync(string query, int top = 5, CancellationToken ct = default)
    {
        EnsureLoaded();

        if (string.IsNullOrWhiteSpace(query))
        {
            return Task.FromResult<IReadOnlyList<TestItem>>(Array.Empty<TestItem>());
        }

        var tokens = Regex.Split(query, @"[^a-zA-Z0-9]+")
            .Select(t => t.Trim().ToLowerInvariant())
            .Where(t => t.Length >= 2 && !Stopwords.Contains(t))
            .Distinct()
            .ToList();

        if (tokens.Count == 0)
        {
            return Task.FromResult<IReadOnlyList<TestItem>>(Array.Empty<TestItem>());
        }

        var scored = new List<(TestItem Item, int Score)>();

        foreach (var test in _tests)
        {
            int score = 0;
            var testName = test.Name.ToLowerInvariant();

            foreach (var token in tokens)
            {
                var wordRegex = new Regex($@"\b{Regex.Escape(token)}\b", RegexOptions.IgnoreCase);

                // Name match weighs more
                if (wordRegex.IsMatch(testName))
                {
                    score += 20;
                }
                else if (testName.Contains(token, StringComparison.OrdinalIgnoreCase))
                {
                    score += 10;
                }

                // Keyword match
                foreach (var kw in test.Keywords)
                {
                    var kwLower = kw.ToLowerInvariant();
                    if (string.Equals(kwLower, token, StringComparison.OrdinalIgnoreCase) || wordRegex.IsMatch(kwLower))
                    {
                        score += 15;
                    }
                    else if (kwLower.Contains(token, StringComparison.OrdinalIgnoreCase))
                    {
                        score += 8;
                    }
                }
            }

            if (score > 0)
            {
                scored.Add((test, score));
            }
        }

        var results = scored
            .OrderByDescending(s => s.Score)
            .Take(top)
            .Select(s => s.Item)
            .ToList();

        return Task.FromResult<IReadOnlyList<TestItem>>(results.AsReadOnly());
    }
}
