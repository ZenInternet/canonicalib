using OpenAI.Chat;
using Zen.CanonicaLib.PackageComparer.Models;

namespace Zen.CanonicaLib.PackageComparer.Services;

public class MigrationGuideGenerator
{
    private readonly string _apiKey;
    private readonly string _model;

    public MigrationGuideGenerator(string apiKey, string model = "gpt-4o")
    {
        _apiKey = apiKey;
        _model = model;
    }

    public async Task<MigrationGuideResult> GenerateMigrationGuideAsync(
        AssemblyComparison comparison,
        string detailedReport)
    {
        var client = new ChatClient(_model, _apiKey);

        // Generate both guides in a single ChatGPT call
        Console.WriteLine($"Generating migration guide and AI prompt with {_model}...");
        var result = await GenerateCombinedGuideAsync(client, comparison, detailedReport);

        return result;
    }

    private async Task<MigrationGuideResult> GenerateCombinedGuideAsync(
        ChatClient client,
        AssemblyComparison comparison,
        string detailedReport)
    {
        var systemPrompt = @"You are an expert technical writer and software architect specializing in API migration documentation and AI-assisted development.
Your task is to create two complementary migration resources:
1. A comprehensive developer migration guide
2. A ready-to-use AI assistant prompt for automated migration

Both should be consistent, accurate, and actionable.";

        var userPrompt = $@"Based on the following package comparison report, create TWO migration resources for upgrading from {comparison.Package1Name} to {comparison.Package2Name}.

COMPARISON REPORT:
{detailedReport}

# RESOURCE 1: DEVELOPER MIGRATION GUIDE

Create a comprehensive markdown migration guide with these sections:

1. **Executive Summary** - Brief overview of changes and **migration effort estimation with detailed reasoning**. When estimating effort:
   - Consider that many changes can be automated with find/replace or AI assistance
   - Namespace changes are typically low-effort (IDE refactoring)
   - Constructor changes may require code review but are often straightforward
   - Only estimate HIGH effort for complex architectural changes or data migrations
   - Be optimistic but realistic - most API upgrades are medium effort at most
   - ALWAYS explain your reasoning for the effort estimate

2. **Breaking Changes** - Detailed list with severity (Critical/High/Medium/Low)
3. **Migration Steps** - Step-by-step guide organized by change type
4. **Code Examples** - Before/After code samples for each breaking change
5. **Namespace Changes** - How to update using statements and fully qualified names
6. **Constructor Changes** - How to update object initialization code
7. **Property/Method Changes** - How to adapt to removed or modified members
8. **Testing Strategy** - Recommended approach to verify the migration
9. **Common Issues** - Known problems and their solutions
10. **Migration Checklist** - Final checklist before deployment
11. **AI-Assisted Migration** - Mention that developers can use the AI prompt from RESOURCE 2

For each code example, provide realistic C# code showing the old pattern and the new pattern.
Use proper markdown formatting with syntax highlighting.

---

# RESOURCE 2: AI ASSISTANT MIGRATION PROMPT

Create a self-contained prompt that developers can copy-paste into GitHub Copilot Chat, Claude, or ChatGPT to automatically migrate their code.

The prompt should:
1. Start with: 'You are helping migrate code from {comparison.Package1Name} to {comparison.Package2Name}. Please perform the following changes...'
2. List all namespace changes with exact find/replace instructions
3. List all constructor signature changes with refactoring instructions
4. List all property/method removals with alternative approaches
5. Include validation steps to verify the migration
6. Handle common edge cases
7. Be ready to use immediately without modification

---

Format your response EXACTLY as follows:

=== DEVELOPER MIGRATION GUIDE ===
[Your complete developer guide here in markdown]

=== AI ASSISTANT PROMPT ===
[Your complete AI prompt here, ready to copy-paste]

=== END ===";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var completion = await client.CompleteChatAsync(messages);
        var fullResponse = completion.Value.Content[0].Text;

        // Parse the response to extract both sections
        var devGuideMarker = "=== DEVELOPER MIGRATION GUIDE ===";
        var aiPromptMarker = "=== AI ASSISTANT PROMPT ===";
        var endMarker = "=== END ===";

        var devGuideStart = fullResponse.IndexOf(devGuideMarker);
        var aiPromptStart = fullResponse.IndexOf(aiPromptMarker);
        var endIndex = fullResponse.IndexOf(endMarker);

        string developerGuide;
        string aiPrompt;

        if (devGuideStart >= 0 && aiPromptStart >= 0)
        {
            developerGuide = fullResponse.Substring(
                devGuideStart + devGuideMarker.Length,
                aiPromptStart - (devGuideStart + devGuideMarker.Length)
            ).Trim();

            if (endIndex >= 0)
            {
                aiPrompt = fullResponse.Substring(
                    aiPromptStart + aiPromptMarker.Length,
                    endIndex - (aiPromptStart + aiPromptMarker.Length)
                ).Trim();
            }
            else
            {
                aiPrompt = fullResponse.Substring(aiPromptStart + aiPromptMarker.Length).Trim();
            }
        }
        else
        {
            // Fallback: use the full response for both
            developerGuide = fullResponse;
            aiPrompt = "Unable to parse AI prompt from response. Please use the developer guide above.";
        }

        return new MigrationGuideResult
        {
            DeveloperGuide = developerGuide,
            AIAssistantPrompt = aiPrompt
        };
    }
}

public class MigrationGuideResult
{
    public required string DeveloperGuide { get; set; }
    public required string AIAssistantPrompt { get; set; }
}
