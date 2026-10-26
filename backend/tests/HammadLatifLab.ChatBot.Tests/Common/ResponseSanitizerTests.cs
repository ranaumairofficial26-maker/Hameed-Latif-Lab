using HammadLatifLab.ChatBot.Application.Common;
using Xunit;

namespace HammadLatifLab.ChatBot.Tests.Common;

public class ResponseSanitizerTests
{
    [Fact]
    public void Clean_StripsGreetingAndWelcomePrefix_WhenUserMessageIsNotGreeting()
    {
        // Requirement 6: Sanitizer: "Hello! Welcome to Hammad Latif Lab. We have 240 tests." for a non-greeting message becomes "We have 240 tests."
        var rawReply = "Hello! Welcome to Hammad Latif Lab. We have 240 tests.";
        var cleaned = ResponseSanitizer.Clean(rawReply, userMessageIsGreeting: false);

        Assert.Equal("We have 240 tests.", cleaned);
    }

    [Fact]
    public void Clean_KeepsShortGreetingReply_WhenUserMessageIsGreeting()
    {
        // Requirement 6: Sanitizer: a greeting-only user message keeps a short greeting reply.
        var greetingReply = "Wa Alaikum Assalam! How can I help you today?";
        var cleaned = ResponseSanitizer.Clean(greetingReply, userMessageIsGreeting: true);

        Assert.Equal(greetingReply, cleaned);
    }

    [Theory]
    [InlineData("hello", true)]
    [InlineData("hello how many tests do you have today please", false)]
    [InlineData("salam", true)]
    [InlineData("CBC price", false)]
    public void IsGreeting_IdentifiesPureGreetingsAccurately(string input, bool expected)
    {
        // Requirement 6: IsGreeting: "hello" true, "hello how many tests do you have today please" false, "salam" true, "CBC price" false.
        var result = ResponseSanitizer.IsGreeting(input);
        Assert.Equal(expected, result);
    }
}
