using HammadLatifLab.ChatBot.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HammadLatifLab.ChatBot.Tests.Repositories;

public class JsonFaqRepositoryTests : IDisposable
{
    private readonly string _tempFile;

    public JsonFaqRepositoryTests()
    {
        _tempFile = Path.GetTempFileName();
        var sampleJson = @"[
            {
                ""id"": 1,
                ""category"": ""Home Sampling"",
                ""question"": ""How can I book home sampling for tests?"",
                ""questionUrdu"": ""Home sampling kaisay book karein?"",
                ""keywords"": [""home sampling"", ""sample collection"", ""blood test""],
                ""answer"": ""Call 042-111-455-522 for home sampling.""
            },
            {
                ""id"": 2,
                ""category"": ""Timings"",
                ""question"": ""What are your opening hours?"",
                ""questionUrdu"": ""Lab timings kya hain?"",
                ""keywords"": [""hours"", ""timings"", ""open""],
                ""answer"": ""We are open 24/7.""
            }
        ]";
        File.WriteAllText(_tempFile, sampleJson);
    }

    public void Dispose()
    {
        if (File.Exists(_tempFile))
        {
            File.Delete(_tempFile);
        }
    }

    [Fact]
    public async Task SearchAsync_WithEnglishTerm_ReturnsMatchingFaq()
    {
        var repo = new JsonFaqRepository(_tempFile, NullLogger<JsonFaqRepository>.Instance);
        var results = await repo.SearchAsync("home sampling");

        Assert.NotEmpty(results);
        Assert.Equal(1, results[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WithRomanUrduTerm_ReturnsMatchingFaq()
    {
        var repo = new JsonFaqRepository(_tempFile, NullLogger<JsonFaqRepository>.Instance);
        var results = await repo.SearchAsync("lab timings kya hain");

        Assert.NotEmpty(results);
        Assert.Equal(2, results[0].Id);
    }

    [Fact]
    public async Task SearchAsync_WithUnrelatedTerm_ReturnsEmpty()
    {
        var repo = new JsonFaqRepository(_tempFile, NullLogger<JsonFaqRepository>.Instance);
        var results = await repo.SearchAsync("car mechanic repair");

        Assert.Empty(results);
    }
}
