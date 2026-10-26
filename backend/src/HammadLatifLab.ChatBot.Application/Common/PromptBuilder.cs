using System.Text;
using HammadLatifLab.ChatBot.Domain.Entities;

namespace HammadLatifLab.ChatBot.Application.Common;

public static class PromptBuilder
{
    public const string DefaultFallbackMessage = 
        "Maazrat, yeh test ya maloomat hamaray mojooda record mein dastiyab nahi hai.\n\n" +
        "Mazeed maloomat, charges ya test booking ke liye barah-e-karam hamari 24/7 Helpline: **+92-42-111-455-522** ya WhatsApp: **0311-1222873** par rabta karein. Hamara amla aapki poori rehnumai ke liye 24/7 hazir hai.";

    public static string GetFallbackMessage(string? userMessage)
    {
        if (string.IsNullOrWhiteSpace(userMessage))
            return DefaultFallbackMessage;

        var lower = userMessage.ToLowerInvariant();
        bool isUrdu = lower.Contains("hai") || lower.Contains("kya") || lower.Contains("kiya") || 
                      lower.Contains("kitna") || lower.Contains("kitne") || lower.Contains("ka") || 
                      lower.Contains("ki") || lower.Contains("ke") || lower.Contains("ko") || 
                      lower.Contains("mein") || lower.Contains("me ") || lower.Contains("bata") || 
                      lower.Contains("karein") || lower.Contains("bataen") || lower.Contains("hain") || 
                      lower.Contains("kab") || lower.Contains("mujhe");

        if (isUrdu)
        {
            return "Maazrat, yeh test ya maloomat hamaray mojooda record mein dastiyab nahi hai.\n\n" +
                   "Mazeed maloomat, charges ya test booking ke liye barah-e-karam hamari 24/7 Helpline: **+92-42-111-455-522** ya WhatsApp: **0311-1222873** par rabta karein. Hamara amla aapki poori rehnumai ke liye 24/7 hazir hai.";
        }

        return "Sorry, this test or information is not available in our current records.\n\n" +
               "For further details, test booking, or queries, please contact our 24/7 Helpline at **+92-42-111-455-522** or WhatsApp at **0311-1222873**. Our team is available 24/7 to assist you.";
    }

    public static string BuildPrompt(
        string userMessage,
        int totalTestsCount,
        IEnumerable<string> allTestNames,
        IEnumerable<TestItem> matchedTests,
        IEnumerable<FaqItem> matchedFaqs,
        IReadOnlyList<(string Role, string Content)>? conversationHistory = null)
    {
        var sb = new StringBuilder();

        sb.AppendLine("You are the assistant of Hammad Latif Lab.");
        sb.AppendLine(" - You may greet and do short, friendly small talk.");
        sb.AppendLine(" - For any lab fact (number of tests, test names, prices, report time, timings,");
        sb.AppendLine("   home sampling, reports) use ONLY the CONTEXT below. Never use your own knowledge.");
        sb.AppendLine(" - If the user asks how many tests we have, answer with the number in LAB OVERVIEW.");
        sb.AppendLine(" - If the user asks about or to list tests, mention a few names (max 10) from");
        sb.AppendLine("   ALL TEST NAMES and say they can ask about any specific test for price and time.");
        sb.AppendLine(" - Price and report time must come ONLY from MATCHED TESTS. Never guess, round or");
        sb.AppendLine("   change a number. If the user asks for a price/time but the test is not in");
        sb.AppendLine("   MATCHED TESTS, politely ask which exact test they mean.");
        sb.AppendLine(" - If several tests are matched, list them (max 5) with price and time.");
        sb.AppendLine(" - If the answer is not in the CONTEXT, say so politely and give the helpline:");
        sb.AppendLine("   +92-42-111-455-522 or WhatsApp 0311-1222873.");
        sb.AppendLine(" - Never give diagnosis, medical advice or medicine suggestions.");
        sb.AppendLine(" - If the question is unrelated to the lab, politely say you can only help with");
        sb.AppendLine("   Hammad Latif Lab services.");
        sb.AppendLine(" - Reply in the same language the user wrote in (English, Urdu or Roman Urdu).");
        sb.AppendLine("   Keep answers short, polite and clear.");
        sb.AppendLine();
        sb.AppendLine("GREETING RULES:");
        sb.AppendLine(" - The chat window already shows a welcome message. NEVER begin a reply with");
        sb.AppendLine("   'Hello', 'Hi', 'Welcome', 'Assalam-o-Alaikum' or any welcome line, and never");
        sb.AppendLine("   say 'Welcome to Hammad Latif Lab'.");
        sb.AppendLine(" - Start directly with the answer.");
        sb.AppendLine(" - Greet back ONLY if the user's current message is itself only a greeting");
        sb.AppendLine("   (e.g. 'hello', 'hi', 'salam', 'assalam o alaikum'), and then reply with ONE");
        sb.AppendLine("   short line, for example: 'Wa Alaikum Assalam! How can I help you?'");
        sb.AppendLine(" - Do not repeat the lab name in every reply. Mention it only when needed.");
        sb.AppendLine(" - Do not end every reply with the same closing sentence. Offer more help only");
        sb.AppendLine("   when it is natural.");

        var historyList = conversationHistory ?? Array.Empty<(string, string)>();
        if (historyList.Count > 0)
        {
            sb.AppendLine(" - This is an ongoing conversation. Do NOT greet or welcome the user again.");
        }

        sb.AppendLine();
        sb.AppendLine("=== CONTEXT ===");
        sb.AppendLine("LAB OVERVIEW:");
        sb.AppendLine($"Total tests available: {totalTestsCount}");
        sb.AppendLine();

        var testNamesList = (allTestNames ?? Enumerable.Empty<string>()).Take(300).ToList();
        sb.AppendLine("ALL TEST NAMES:");
        sb.AppendLine(string.Join("; ", testNamesList));
        sb.AppendLine();

        var matchedTestsList = (matchedTests ?? Enumerable.Empty<TestItem>()).ToList();
        if (matchedTestsList.Count > 0)
        {
            sb.AppendLine("MATCHED TESTS:");
            foreach (var test in matchedTestsList)
            {
                sb.AppendLine($"Test: {test.Name} | Report time: {test.Time} | Price: Rs. {test.Price}");
            }
            sb.AppendLine();
        }

        var matchedFaqsList = (matchedFaqs ?? Enumerable.Empty<FaqItem>()).ToList();
        if (matchedFaqsList.Count > 0)
        {
            sb.AppendLine("MATCHED FAQS:");
            foreach (var faq in matchedFaqsList)
            {
                sb.AppendLine($"Q: {faq.Question} | A: {faq.Answer}");
            }
            sb.AppendLine();
        }

        if (historyList.Count > 0)
        {
            sb.AppendLine("=== CONVERSATION SO FAR ===");
            foreach (var (role, content) in historyList)
            {
                sb.AppendLine($"{role}: {content}");
            }
            sb.AppendLine();
        }

        sb.AppendLine("=== USER MESSAGE ===");
        sb.AppendLine(userMessage);

        return sb.ToString();
    }
}
