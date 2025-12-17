using OpenAI.Chat;
using Zen.CanonicaLib.PackageComparer.Models;

namespace Zen.CanonicaLib.PackageComparer.Services;

public class MigrationGuideGenerator
{
    private readonly string _apiKey;

    public MigrationGuideGenerator(string apiKey)
    {
        _apiKey = apiKey;
    }

    public async Task<MigrationGuideResult> GenerateMigrationGuideAsync(
        AssemblyComparison comparison,
        string detailedReport)
    {
        var client = new ChatClient("gpt-4o", _apiKey);

        // Generate developer migration guide
        Console.WriteLine("Generating developer migration guide with ChatGPT...");
        var migrationGuide = await GenerateDeveloperGuideAsync(client, comparison, detailedReport);

        // Generate AI assistant prompt
        Console.WriteLine("Generating AI assistant migration prompt...");
        var aiPrompt = await GenerateAIPromptAsync(client, comparison, detailedReport);

        return new MigrationGuideResult
        {
            DeveloperGuide = migrationGuide,
            AIAssistantPrompt = aiPrompt
        };
    }

    private async Task<string> GenerateDeveloperGuideAsync(
        ChatClient client,
        AssemblyComparison comparison,
        string detailedReport)
    {
        var systemPrompt = @"You are an expert technical writer and software architect specializing in API migration documentation. 
Your task is to create comprehensive, developer-friendly migration guides that help teams upgrade from one version of a package to another.

Focus on:
- Clear, actionable migration steps
- Code examples showing before/after patterns
- Breaking changes highlighted with severity levels
- Common pitfalls and solutions
- Migration strategies (big bang vs incremental)
- Testing recommendations

Use clear markdown formatting with proper headings, code blocks, tables, and callouts.";

        var userPrompt = $@"Based on the following package comparison report, create a comprehensive migration guide for developers upgrading from {comparison.Package1Name} to {comparison.Package2Name}.

COMPARISON REPORT:
{detailedReport}

Please create a migration guide that includes:

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

For each code example, provide realistic C# code showing the old pattern and the new pattern.
Use proper markdown formatting with syntax highlighting.";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var completion = await client.CompleteChatAsync(messages);
        return completion.Value.Content[0].Text;
    }

    private async Task<string> GenerateAIPromptAsync(
        ChatClient client,
        AssemblyComparison comparison,
        string detailedReport)
    {
        var systemPrompt = @"You are an expert at creating prompts for AI coding assistants like GitHub Copilot, Claude, and ChatGPT.
Your task is to create a comprehensive, single-prompt instruction that an AI assistant can use to automatically perform code migrations.

The prompt should be:
- Self-contained and complete
- Specific about search patterns and replacements
- Include validation steps
- Handle edge cases
- Be formatted for easy copy-paste into AI assistants";

        var userPrompt = $@"Based on the following package comparison report, create a comprehensive prompt that developers can give to AI coding assistants (GitHub Copilot, Claude, etc.) to automatically migrate their codebase from {comparison.Package1Name} to {comparison.Package2Name}.

COMPARISON REPORT:
{detailedReport}

Create a prompt that:

1. Explains the migration context and scope
2. Lists all namespace changes with find/replace instructions
3. Lists all constructor signature changes with refactoring instructions
4. Lists all property/method removals with alternative approaches
5. Includes validation steps to verify the migration
6. Handles common edge cases

The prompt should be:
- Ready to copy-paste into GitHub Copilot Chat, Claude, or ChatGPT
- Self-contained (no external references needed)
- Specific with exact type names and namespaces
- Include examples of what to look for and how to fix it

Format the output as a single, cohesive prompt that starts with something like:
'You are helping migrate code from [Package] version X to version Y. Please perform the following changes...'";

        var messages = new List<ChatMessage>
        {
            new SystemChatMessage(systemPrompt),
            new UserChatMessage(userPrompt)
        };

        var completion = await client.CompleteChatAsync(messages);
        return completion.Value.Content[0].Text;
    }
}

public class MigrationGuideResult
{
    public required string DeveloperGuide { get; set; }
    public required string AIAssistantPrompt { get; set; }
}
