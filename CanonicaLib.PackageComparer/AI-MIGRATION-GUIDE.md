# AI Migration Guide Generation - Usage Examples

## Overview

The CanonicaLib Package Comparer can generate comprehensive AI-powered migration guides using OpenAI's ChatGPT API.

## Prerequisites

1. OpenAI API key (get one from https://platform.openai.com/api-keys)
2. Set up billing in your OpenAI account (GPT-4 API access required)

## Basic Usage

```powershell
# Compare two packages and generate migration guides
dotnet run --project CanonicaLib.PackageComparer -- \
  "OldPackage/1.0.0" \
  "NewPackage/2.0.0" \
  --api-key sk-your-openai-api-key-here \
  --migration-guide ./migration-guide
```

This generates two files:
- `migration-guide-developer.md` - Human-readable migration guide
- `migration-guide-ai-prompt.txt` - AI assistant prompt

## Complete Example with Local Packages

```powershell
dotnet run --project CanonicaLib.PackageComparer -- \
  "D:\Temp\Zen.Contract.Orders.0.9.14.1.nupkg" \
  "D:\Temp\Zen.Contract.Orders.0.9.15-rc.nupkg" \
  --verbose \
  --format markdown \
  --output comparison-report.md \
  --api-key sk-your-api-key \
  --migration-guide ./zen-orders-migration
```

Output:
- `comparison-report.md` - Standard comparison report
- `zen-orders-migration-developer.md` - Developer migration guide with code examples
- `zen-orders-migration-ai-prompt.txt` - Prompt for AI coding assistants

## Using the AI Assistant Prompt

Once generated, you can use the AI prompt with various tools:

### With GitHub Copilot Chat

1. Open VS Code
2. Open Copilot Chat
3. Paste the contents of `*-ai-prompt.txt`
4. Let Copilot perform the migration

### With Claude (Anthropic)

1. Open Claude chat
2. Paste the prompt
3. Provide your codebase context
4. Claude will guide you through the migration

### With ChatGPT

1. Open ChatGPT
2. Paste the prompt
3. Share code snippets that need migration
4. Get specific refactoring suggestions

## Environment Variable for API Key

For security, you can use an environment variable instead of passing the key on command line:

```powershell
# Set environment variable
$env:OPENAI_API_KEY = "sk-your-api-key-here"

# Use in the tool (modify code to read from environment)
dotnet run --project CanonicaLib.PackageComparer -- \
  package1/1.0 package2/2.0 \
  --api-key $env:OPENAI_API_KEY \
  --migration-guide ./migration
```

## What Gets Generated

### Developer Migration Guide (`*-developer.md`)

Comprehensive markdown document with:
- **Executive Summary** - Migration complexity and effort estimation
- **Breaking Changes** - Categorized by severity (Critical/High/Medium/Low)
- **Step-by-Step Instructions** - Organized migration workflow
- **Code Examples** - Before/After for every change:
  ```csharp
  // Before (v1.0)
  var order = new CommercialOrderItemSummary("123", "Product");
  
  // After (v2.0)
  var order = new CommercialOrderItemSummary 
  { 
      Id = "123", 
      Name = "Product" 
  };
  ```
- **Namespace Changes** - Find/replace patterns
- **Testing Strategy** - How to verify migration
- **Common Pitfalls** - Known issues and solutions
- **Migration Checklist** - Final verification steps

### AI Assistant Prompt (`*-ai-prompt.txt`)

Ready-to-use prompt containing:
- Migration context and scope
- All namespace changes with exact patterns
- Constructor signature updates
- Property/method changes
- Validation steps
- Edge case handling

Example snippet from generated prompt:
```
You are helping migrate code from Zen.Contract.Orders 0.9.14.1 to version 0.9.15-rc.

Please perform the following changes:

1. NAMESPACE CHANGES:
   - Find: using Zen.Contract.Orders;
   - Replace with: using Zen.Contract.Orders.Enums;
   - Applies to: CommercialOrderSource, FulfilmentAction, FulfilmentIntent...

2. CONSTRUCTOR CHANGES:
   - CommercialOrderItemSummary:
     Old: new CommercialOrderItemSummary(id, name)
     New: new CommercialOrderItemSummary { Id = id, Name = name }
...
```

## Cost Estimation

Using GPT-4 API:
- Small package comparison: ~$0.05-0.15 per report
- Medium package comparison: ~$0.15-0.30 per report
- Large package comparison: ~$0.30-0.50 per report

Actual costs depend on:
- Number of types changed
- Complexity of changes
- Level of detail in generated examples

## Tips

1. **Use verbose mode** for better AI context: `-v` flag
2. **Generate markdown report** alongside migration guide: `-f markdown -o report.md`
3. **Review AI output** before applying - AI can make mistakes
4. **Test incrementally** - don't migrate everything at once
5. **Keep API key secure** - never commit to source control
