# CanonicaLib Package Comparer

A command-line tool for comparing two NuGet packages and performing gap analysis on their public APIs.

## Features

- Download and extract NuGet packages from NuGet.org or use local .nupkg files
- **Latest Version Resolution** - Use `PackageId/latest` to compare against the latest stable version
- **Authenticated Feed Support** - Works with Azure DevOps, private NuGet feeds, and custom sources
- **Compare DLL files directly** - No need to package them first
- Compare all public types (classes, interfaces, enums, structs, delegates)
- Detect added, removed, and modified members
- Detect namespace changes automatically
- Compare attributes/annotations on types and members
- Identify breaking changes
- Generate reports in multiple formats (text, markdown, JSON)
- **AI-Powered Migration Guides** - Generate comprehensive migration documentation using ChatGPT

## Usage

### Compare two packages from NuGet.org

```powershell
dotnet run --project CanonicaLib.PackageComparer -- Newtonsoft.Json/13.0.1 Newtonsoft.Json/13.0.3
```

### Compare against latest stable version

Use `latest` to automatically resolve to the latest non-prerelease version:

```powershell
# Compare specific version against latest
dotnet run --project CanonicaLib.PackageComparer -- Newtonsoft.Json/12.0.0 Newtonsoft.Json/latest

# Compare latest from two different packages
canonicalib-comparer EntityFramework/latest Microsoft.EntityFrameworkCore/latest
```

### Compare local .nupkg files

```powershell
dotnet run --project CanonicaLib.PackageComparer -- package1.nupkg package2.nupkg
```

### Compare DLL files directly

```powershell
dotnet run --project CanonicaLib.PackageComparer -- MyLibrary.v1.dll MyLibrary.v2.dll
```

### Mix and match

```powershell
# Compare a local DLL against a NuGet package
dotnet run --project CanonicaLib.PackageComparer -- MyLibrary.dll Newtonsoft.Json/13.0.3

# Compare a .nupkg against a DLL
dotnet run --project CanonicaLib.PackageComparer -- package.nupkg assembly.dll
```

### Package Identifier Format

When specifying packages from NuGet feeds, use one of these formats:

- `PackageId/Version` - Specific version (e.g., `Newtonsoft.Json/13.0.1`)
- `PackageId/latest` - Latest stable (non-prerelease) version
- Path to `.nupkg` file - Local package file
- Path to `.dll` file - Direct assembly comparison

### Options

- `-o, --output <path>` - Write report to file instead of console
- `-f, --format <format>` - Output format: text (default), markdown, or json
- `-v, --verbose` - Show detailed comparison information
- `-s, --source <url>` - Custom NuGet package source URL
- `-k, --api-key <key>` - OpenAI API key for generating AI-powered migration guides
- `-m, --migration-guide <path>` - Output path for migration guide files (requires --api-key)

### Examples

```powershell
# Detailed comparison with markdown output
dotnet run --project CanonicaLib.PackageComparer -- MyPackage/1.0.0 MyPackage/2.0.0 -f markdown -v -o comparison.md

# JSON output for automation
dotnet run --project CanonicaLib.PackageComparer -- Package1/1.0 Package2/1.0 -f json -o report.json

# Compare current version against latest stable
canonicalib-comparer MyPackage/1.5.0 MyPackage/latest -f markdown -o upgrade-analysis.md

# Using custom NuGet source
dotnet run --project CanonicaLib.PackageComparer -- MyPackage/1.0 MyPackage/2.0 -s https://my-nuget-feed.com/v3/index.json

# Using authenticated feeds (Azure DevOps, private feeds)
# The tool automatically uses credentials from nuget.config - see AUTHENTICATED-FEEDS.md for details
canonicalib-comparer MyPrivatePackage/1.0 MyPrivatePackage/2.0

# Generate AI-powered migration guide
dotnet run --project CanonicaLib.PackageComparer -- OldPackage/1.0 NewPackage/2.0 -k YOUR_OPENAI_API_KEY -m migration-guide
```

## AI-Powered Migration Guides

When you provide an OpenAI API key, the tool can generate two AI-powered documents:

1. **Developer Migration Guide** (`*-developer.md`) - A comprehensive markdown document containing:
   - Executive summary with migration effort estimation
   - Breaking changes categorized by severity
   - Step-by-step migration instructions
   - Before/after code examples for each change
   - Testing strategy and validation steps
   - Common issues and solutions
   - Migration checklist

2. **AI Assistant Prompt** (`*-ai-prompt.txt`) - A ready-to-use prompt for AI coding assistants:
   - Complete instructions for GitHub Copilot, Claude, or ChatGPT
   - Specific find/replace patterns for all changes
   - Namespace migration instructions
   - Constructor and member refactoring steps
   - Validation and edge case handling

### Setting Up Your API Key

You have **three options** for providing your OpenAI API key (checked in this priority order):

**Option 1: Command Line** (highest priority)
```powershell
dotnet run -- package1.nupkg package2.nupkg --api-key sk-your-key --migration-guide ./guides
```

**Option 2: Environment Variable**
```powershell
$env:OPENAI_API_KEY = "sk-your-openai-api-key-here"
dotnet run -- package1.nupkg package2.nupkg --migration-guide ./guides
```

**Option 3: Configuration File** (⭐ recommended for local development)
1. Create/edit `appsettings.Development.json`:
```json
{
  "OpenAI": {
    "ApiKey": "sk-your-openai-api-key-here"
  }
}
```
2. This file is automatically excluded from git (protected by `.gitignore`)
3. Run without specifying the key:
```powershell
dotnet run -- package1.nupkg package2.nupkg --migration-guide ./guides
```

### Example Usage

```powershell
# Generate migration guides (using appsettings.Development.json)
dotnet run --project CanonicaLib.PackageComparer -- \
  "D:\Temp\OldPackage.1.0.0.nupkg" \
  "D:\Temp\NewPackage.2.0.0.nupkg" \
  --migration-guide ./guides/migration

# This creates:
# - ./guides/migration-developer.md (for human developers)
# - ./guides/migration-ai-prompt.txt (for AI assistants)
```

Then you can:
1. Share the developer guide with your team
2. Copy the AI prompt into GitHub Copilot Chat or Claude to automatically migrate your codebase

## Report Output

The tool generates comprehensive gap analysis reports showing:

- Summary statistics (identical, modified, added, removed types)
- Assembly information for both packages
- Types removed (present in package 1, missing in package 2)
- Types added (new in package 2)
- Types modified with detailed member-level changes
- Conclusion about breaking changes

## Building the Tool

```powershell
dotnet build CanonicaLib.PackageComparer
```

## Publishing as Executable

```powershell
dotnet publish CanonicaLib.PackageComparer -c Release -o ./publish
```
