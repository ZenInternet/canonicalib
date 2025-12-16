# CanonicaLib Package Comparer

A command-line tool for comparing two NuGet packages and performing gap analysis on their public APIs.

## Features

- Download and extract NuGet packages from NuGet.org or use local .nupkg files
- Compare all public types (classes, interfaces, enums, structs, delegates)
- Detect added, removed, and modified members
- Identify breaking changes
- Generate reports in multiple formats (text, markdown, JSON)

## Usage

### Compare two packages from NuGet.org

```powershell
dotnet run --project CanonicaLib.PackageComparer -- Newtonsoft.Json/13.0.1 Newtonsoft.Json/13.0.3
```

### Compare local .nupkg files

```powershell
dotnet run --project CanonicaLib.PackageComparer -- package1.nupkg package2.nupkg
```

### Options

- `-o, --output <path>` - Write report to file instead of console
- `-f, --format <format>` - Output format: text (default), markdown, or json
- `-v, --verbose` - Show detailed comparison information
- `-s, --source <url>` - Custom NuGet package source URL

### Examples

```powershell
# Detailed comparison with markdown output
dotnet run --project CanonicaLib.PackageComparer -- MyPackage/1.0.0 MyPackage/2.0.0 -f markdown -v -o comparison.md

# JSON output for automation
dotnet run --project CanonicaLib.PackageComparer -- Package1/1.0 Package2/1.0 -f json -o report.json

# Using custom NuGet source
dotnet run --project CanonicaLib.PackageComparer -- MyPackage/1.0 MyPackage/2.0 -s https://my-nuget-feed.com/v3/index.json
```

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
