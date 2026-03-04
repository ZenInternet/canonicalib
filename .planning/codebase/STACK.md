# Technology Stack

**Analysis Date:** 2026-02-17

## Languages

**Primary:**
- C# 12+ - All projects use modern C# with nullable reference types enabled, implicit usings, and top-level statements

## Runtime

**Environment:**
- .NET 8.0 - Primary framework for most projects
- .NET Standard 2.1 - Used for `CanonicaLib.DataAnnotations` to maximize compatibility

**Package Manager:**
- NuGet (.NET package manager)
- Lockfile: `*.csproj` files contain explicit version constraints using floating versions (e.g., `[2.3.9,)`, `[8.0.21,)`)

## Frameworks

**Core:**
- ASP.NET Core 8 - Framework reference for web components in `CanonicaLib.UI`
- Microsoft.NET.Sdk - Build SDK for all projects

**Testing:**
- xUnit 2.6.2 - Test framework in `CanonicaLib.UI.Tests`
- Moq 4.20.70 - Mocking framework for unit tests
- FluentAssertions 6.12.0 - Fluent assertion library
- coverlet.collector 6.0.0 - Code coverage collection

**Build/Dev:**
- MinVer 6.0.0 - Automatic semantic versioning from git tags (configured in `Directory.Build.props`)
- Microsoft.CodeAnalysis.NetAnalyzers 9.0.0 - Code quality analysis (Debug only)
- SonarAnalyzer.CSharp - Enhanced code quality analysis (Debug only)
- System.CommandLine 2.0.0-beta4.22272.1 - CLI argument parsing for `CanonicaLib.PackageComparer`

## Key Dependencies

**Critical:**
- Microsoft.OpenApi 2.3.9+ - OpenAPI specification generation and manipulation across `CanonicaLib.UI` and `CanonicaLib.DataAnnotations`
- Namotion.Reflection 3.4.3+ - Advanced reflection utilities for runtime type analysis in schema generation
- Microsoft.AspNetCore.Mvc.Core 2.3.0+ - MVC abstractions for attribute definitions
- Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation 8.0.21+ - Runtime Razor compilation for dynamic view generation in `CanonicaLib.UI`

**Infrastructure:**
- NuGet.Configuration 6.11.1 - NuGet configuration management
- NuGet.Credentials 6.11.1 - Credential handling for private NuGet sources
- NuGet.Packaging 6.11.1 - Package metadata and creation
- NuGet.Protocol 6.11.1 - NuGet protocol implementation for package resolution
- System.Reflection.Metadata 8.0.0 - Metadata reading without loading assemblies
- System.Reflection.MetadataLoadContext 8.0.0 - Assembly reflection context isolation in `CanonicaLib.PackageComparer`
- OpenAI 2.1.0 - OpenAI API client for AI-powered migration guide generation in `CanonicaLib.PackageComparer`
- Microsoft.Extensions.Configuration 8.0.0 - Configuration management
- Microsoft.Extensions.Configuration.Json 8.0.0 - JSON configuration support
- Microsoft.Extensions.Configuration.EnvironmentVariables 8.0.0 - Environment variable configuration

## Configuration

**Environment:**
- Configuration system: Microsoft.Extensions.Configuration with JSON, environment variables, and appsettings files
- Startup file: `CanonicaLib.PackageComparer/appsettings.json` contains OpenAI API configuration structure
- Environment-specific configs: Supports `appsettings.Development.json` for local overrides

**Build:**
- Solution file: `canonicalib.sln` (Visual Studio 17+)
- Project files: `*.csproj` files with explicit target frameworks and package metadata
- Directory-level configuration: `Directory.Build.props` centrally configures MinVer versioning and package icons

**Code Quality:**
- Nullable reference types: Enabled in all projects (`<Nullable>enable</Nullable>`)
- Treat warnings as errors: `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` enforces strict compilation
- Documentation file generation: Enabled with XML documentation
- Code style enforcement: `<EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>`
- .NET Analyzers: Latest analysis level enabled (`<AnalysisLevel>latest</AnalysisLevel>`)

## Platform Requirements

**Development:**
- .NET SDK 8.0.x or later
- Visual Studio 2022+ (IDE) with C# support
- Windows, macOS, or Linux with .NET SDK installed

**Production:**
- .NET Runtime 8.0+ for library consumption
- Self-contained executables built for: Windows x64, Linux x64, macOS x64 (Intel), macOS ARM64 (Apple Silicon)
- Single-file publishing enabled for standalone tool distribution

## Package Publishing

**Target:**
- NuGet.org as primary registry
- Published packages:
  - `Zen.CanonicaLib.DataAnnotations` - Core library package (net8.0, netstandard2.1)
  - `Zen.CanonicaLib.UI` - OpenAPI generation package (net8.0)
  - `Zen.CanonicaLib.PackageComparer` - CLI tool with standalone executables

**Versioning:**
- MinVer drives automatic versioning from git tags (`v*` format, `MinVerMinimumMajorMinor=1.1`)
- Pre-release packages: `-prerelease.{build-number}` suffix for next branch
- Release candidates: `-rc.{build-number}` suffix for PRs to main
- Stable releases: Bumped minor version on push to main

---

*Stack analysis: 2026-02-17*
