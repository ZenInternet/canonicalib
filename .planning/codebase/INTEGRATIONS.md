# External Integrations

**Analysis Date:** 2026-02-17

## APIs & External Services

**OpenAI API:**
- Service: OpenAI GPT models for AI-powered migration guide generation
- What it's used for: Generating comprehensive migration guides and AI-assistant prompts when comparing NuGet packages
- SDK/Client: `OpenAI` v2.1.0 package
- Auth: API key via `--api-key` CLI parameter, `OPENAI_API_KEY` environment variable, or `appsettings.json` config
- Implementation: `CanonicaLib.PackageComparer/Services/MigrationGuideGenerator.cs`
- Default model: `gpt-4o` (configurable via `--model` CLI parameter)
- Supported models: `gpt-4o`, `gpt-4o-mini`, `gpt-4-turbo`, or any OpenAI chat model

**NuGet Package Registry:**
- Service: NuGet.org default source for package resolution
- What it's used for: Downloading and comparing NuGet packages by ID and version
- SDK/Client: `NuGet.Protocol` v6.11.1, `NuGet.Packaging` v6.11.1, `NuGet.Configuration` v6.11.1
- Auth: Optional private source support via `NuGet.Credentials`
- Implementation: `CanonicaLib.PackageComparer/Services/PackageExtractor.cs`
- Custom source: Configurable via `--source` CLI parameter

## Data Storage

**Databases:**
- Not applicable - CanonicaLib is a library and CLI tool without persistent storage

**File Storage:**
- Local filesystem only
- Temporary extraction directories created during package analysis
- Output files: Comparison reports and migration guides written to specified paths
- Configuration files: `appsettings.json` and `appsettings.Development.json` stored locally

**Caching:**
- None - No caching mechanism implemented

## Authentication & Identity

**Auth Provider:**
- Custom configuration-based approach
- OpenAI API: API key resolution (lowest to highest priority):
  1. `--api-key` CLI parameter
  2. `OPENAI_API_KEY` environment variable
  3. `appsettings.json` configuration file (`OpenAI:ApiKey` section)
- NuGet private sources: Support via `NuGet.Credentials` for authenticated feeds

## Monitoring & Observability

**Error Tracking:**
- Not integrated - Errors logged to console stderr

**Logs:**
- Console-based logging (stdout/stderr)
- Progress messages written to console during package extraction and analysis
- Verbose mode (`--verbose` flag) in `CanonicaLib.PackageComparer` for detailed diagnostics
- Error details include stack traces when verbose flag is enabled

## CI/CD & Deployment

**Hosting:**
- Published to NuGet.org public registry
- GitHub Releases with standalone executables for Windows, Linux, and macOS
- .NET Global Tool distribution via NuGet package

**CI Pipeline:**
- GitHub Actions workflow: `.github/workflows/build-and-publish.yml`
- Triggers: Pushes to `main` and `next` branches, feature branches, and git tags (`v*`)
- Build jobs:
  - Restore dependencies via `dotnet restore`
  - Build with `dotnet build` in Release configuration
  - Run tests with `dotnet test`
  - Pack NuGet packages with `dotnet pack`
  - Publish standalone executables with `dotnet publish` for multiple platforms
  - Publish packages to NuGet.org with `dotnet nuget push`
- Pre-release publishing: Automatic for `next` branch pushes and PRs to `next`
- RC publishing: Automatic for PRs to `main` with GitHub Release creation
- Stable releasing: Automatic tag creation and package publishing on `main` pushes
- Version management: MinVer automatic versioning from git tags and branch context

## Environment Configuration

**Required env vars:**
- `OPENAI_API_KEY` (optional) - OpenAI API key for migration guide generation
- `NUGET_API_KEY` (GitHub Secrets in CI) - NuGet authentication for package publishing

**Secrets location:**
- GitHub Repository Secrets for CI/CD:
  - `NUGET_API_KEY` - Used in `.github/workflows/build-and-publish.yml` for package publishing
  - `GITHUB_TOKEN` - Used for creating GitHub releases

**Configuration files:**
- `CanonicaLib.PackageComparer/appsettings.json` - Default OpenAI configuration template
- `CanonicaLib.PackageComparer/appsettings.Development.json` - Local development overrides
- Both files copied to output directory with `<CopyToOutputDirectory>PreserveNewest</CopyToOutputDirectory>`

## Webhooks & Callbacks

**Incoming:**
- None - Library and CLI tool without webhook endpoints

**Outgoing:**
- NuGet.org push notifications (implicit from `dotnet nuget push`)
- GitHub API calls for release creation via `softprops/action-gh-release@v1`

## Assembly Analysis Integration

**Reflection and Metadata:**
- `System.Reflection.MetadataLoadContext` - Isolated assembly loading without loading into current app domain
- `System.Reflection.Metadata` - Low-level metadata reading from DLLs
- `Namotion.Reflection` - Advanced reflection utilities for type analysis
- Implementation: `CanonicaLib.PackageComparer/Services/AssemblyAnalyzer.cs`
- Purpose: Compare public APIs between two different package versions

## OpenAPI Integration

**OpenAPI Specification:**
- `Microsoft.OpenApi` v2.3.9+ - Core library for OpenAPI document generation and manipulation
- Generates OpenAPI v3.0/v3.1 specifications from attributed C# interfaces and classes
- Implementation: `CanonicaLib.UI/Services/DefaultDocumentGenerator.cs`
- Extensions provided: Custom OpenAPI extensions for tag groups, validation results, and security
- Razor view compilation: `Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation` for dynamic UI rendering

---

*Integration audit: 2026-02-17*
