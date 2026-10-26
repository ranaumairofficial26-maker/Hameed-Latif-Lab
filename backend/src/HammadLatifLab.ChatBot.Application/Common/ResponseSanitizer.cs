using System.Text.RegularExpressions;

namespace HammadLatifLab.ChatBot.Application.Common;

public static class ResponseSanitizer
{
    private static readonly string[] GreetingPrefixes = new[]
    {
        "hello", "hi", "hey", "salam", "assalam o alaikum", "assalam-o-alaikum", "assalamualaikum", "aoa", "good morning", "good evening"
    };

    public static bool IsGreeting(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
            return false;

        var cleaned = Regex.Replace(message.Trim().ToLowerInvariant(), @"[^\w\s-]", " ").Trim();
        var words = cleaned.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

        // Only a short message (up to 6 words) counts as a greeting
        if (words.Length == 0 || words.Length > 6)
            return false;

        var fullJoined = string.Join(" ", words);

        foreach (var prefix in GreetingPrefixes)
        {
            if (fullJoined == prefix || fullJoined.StartsWith(prefix + " "))
            {
                // If the message contains explicit test or pricing intent, treat as query rather than pure greeting
                if (fullJoined.Contains("test") || fullJoined.Contains("tests") || fullJoined.Contains("price") || 
                    fullJoined.Contains("rate") || fullJoined.Contains("how many") || fullJoined.Contains("kitna"))
                {
                    return false;
                }
                return true;
            }
        }

        return false;
    }

    public static string Clean(string reply, bool userMessageIsGreeting)
    {
        if (string.IsNullOrWhiteSpace(reply))
            return reply ?? string.Empty;

        // If user message was solely a greeting, allow the short greeting reply
        if (userMessageIsGreeting)
            return reply;

        var current = reply.Trim();

        // Regex to strip greetings and welcome prefixes from start
        var greetingRegex = new Regex(@"^\s*(hello|hi|hey|assalam[- ]?o[- ]?alaikum|assalamualaikum|aoa|salam|welcome(\s+to\s+[^.!\n\r]*)?)[\s,!.\-:]*", RegexOptions.IgnoreCase);

        // Repeat twice in case two greeting lines are stacked (e.g. "Hello! Welcome to Hammad Latif Lab. ...")
        for (int i = 0; i < 2; i++)
        {
            var match = greetingRegex.Match(current);
            if (match.Success && match.Length > 0)
            {
                current = current.Substring(match.Length).Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(current))
        {
            return reply;
        }

        // Capitalize the first letter
        if (current.Length > 1)
        {
            current = char.ToUpperInvariant(current[0]) + current.Substring(1);
        }
        else if (current.Length == 1)
        {
            current = current.ToUpperInvariant();
        }

        return current;
    }
}
