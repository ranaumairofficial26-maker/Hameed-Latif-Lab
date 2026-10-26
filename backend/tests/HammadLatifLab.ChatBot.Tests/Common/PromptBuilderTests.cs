using HammadLatifLab.ChatBot.Application.Common;
using HammadLatifLab.ChatBot.Domain.Entities;
using Xunit;

namespace HammadLatifLab.ChatBot.Tests.Common;

public class PromptBuilderTests
{
    [Fact]
    public void BuildPrompt_IncludesOverviewAllNamesAndMatchedContext()
    {
        var faqs = new List<FaqItem>
        {
            new()
            {
                Id = 1,
                Category = "Test",
                Question = "How to book?",
                Answer = "Call our hotline.",
                Keywords = new List<string> { "book" }
            }
        };

        var matchedTests = new List<TestItem>
        {
            new()
            {
                Id = 1,
                Name = "Blood Glucose Fasting",
                Price = 600,
                Time = "After 24 Hours",
                Keywords = new List<string> { "glucose" }
            }
        };

        var allTestNames = new List<string> { "Blood Glucose Fasting", "Complete Blood Count (CBC)" };

        var prompt = PromptBuilder.BuildPrompt(
            "How to book a test?",
            totalTestsCount: 752,
            allTestNames: allTestNames,
            matchedTests: matchedTests,
            matchedFaqs: faqs);

        Assert.Contains("You are the assistant of Hammad Latif Lab.", prompt);
        Assert.Contains("Total tests available: 752", prompt);
        Assert.Contains("Blood Glucose Fasting; Complete Blood Count (CBC)", prompt);
        Assert.Contains("Test: Blood Glucose Fasting | Report time: After 24 Hours | Price: Rs. 600", prompt);
        Assert.Contains("Q: How to book? | A: Call our hotline.", prompt);
        Assert.Contains("=== USER MESSAGE ===", prompt);
    }
}
