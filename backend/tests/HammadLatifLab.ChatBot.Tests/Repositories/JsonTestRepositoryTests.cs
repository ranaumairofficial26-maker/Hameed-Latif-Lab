using HammadLatifLab.ChatBot.Infrastructure.Repositories;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace HammadLatifLab.ChatBot.Tests.Repositories;

public class JsonTestRepositoryTests : IDisposable
{
    private readonly string _tempFile;

    public JsonTestRepositoryTests()
    {
        _tempFile = Path.GetTempFileName();
        var sampleJson = @"[
            {
                ""id"": 1,
                ""name"": ""Blood Glucose (Fasting)"",
                ""time"": ""After 24 Hours"",
                ""price"": 600,
                ""keywords"": [""blood"", ""glucose"", ""fasting"", ""sugar""]
            },
            {
                ""id"": 2,
                ""name"": ""Complete Blood Count (CBC)"",
                ""time"": ""Same day"",
                ""price"": 900,
                ""keywords"": [""cbc"", ""blood count"", ""complete blood""]
            },
            {
                ""id"": 3,
                ""name"": ""Lipid Profile"",
                ""time"": ""After 24 Hours"",
                ""price"": 2900,
                ""keywords"": [""lipid"", ""profile"", ""cholesterol""]
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
    public async Task SearchAsync_WithExactName_ReturnsMatchingTest()
    {
        var repo = new JsonTestRepository(_tempFile, NullLogger<JsonTestRepository>.Instance);
        var results = await repo.SearchAsync("Lipid Profile");

        Assert.NotEmpty(results);
        Assert.Equal("Lipid Profile", results[0].Name);
        Assert.Equal(2900, results[0].Price);
    }

    [Fact]
    public async Task SearchAsync_WithSentenceFullOfStopwords_FindsCBC()
    {
        var repo = new JsonTestRepository(_tempFile, NullLogger<JsonTestRepository>.Instance);
        // Requirement 6: Search with a sentence full of stopwords still finds "CBC" in "can u please tell me the price of cbc"
        var results = await repo.SearchAsync("can u please tell me the price of cbc");

        Assert.NotEmpty(results);
        Assert.Contains(results, t => t.Name.Contains("CBC") || t.Keywords.Contains("cbc"));
    }

    [Fact]
    public async Task SearchAsync_WithOnlyStopwords_ReturnsEmptyListWithoutError()
    {
        var repo = new JsonTestRepository(_tempFile, NullLogger<JsonTestRepository>.Instance);
        // Requirement 6: Search with only stopwords returns empty list without error
        var results = await repo.SearchAsync("can u please tell me about the test how many do you have");

        Assert.NotNull(results);
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchAsync_WithRomanUrduQuery_ReturnsMatchingTest()
    {
        var repo = new JsonTestRepository(_tempFile, NullLogger<JsonTestRepository>.Instance);
        var results = await repo.SearchAsync("sugar test kitne ka hai aur rate kya hai");

        Assert.NotEmpty(results);
        Assert.Contains(results, t => t.Name.Contains("Glucose") || t.Keywords.Contains("sugar"));
    }

    [Fact]
    public async Task SearchAsync_WithNoMatchQuery_ReturnsEmptyList()
    {
        var repo = new JsonTestRepository(_tempFile, NullLogger<JsonTestRepository>.Instance);
        var results = await repo.SearchAsync("car mechanic transmission repair");

        Assert.Empty(results);
    }
}
