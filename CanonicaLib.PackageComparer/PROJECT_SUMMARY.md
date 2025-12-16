# CanonicaLib.PackageComparer - Project Summary

## Overview

The CanonicaLib.PackageComparer is a command-line executable tool that performs gap analysis between two NuGet packages by extracting and comparing their DLL contents.

## Project Structure

```
CanonicaLib.PackageComparer/
├── Program.cs                          # Entry point with CLI argument handling
├── CanonicaLib.PackageComparer.csproj  # Project file with dependencies
├── README.md                           # User documentation
├── examples.bat                        # Example usage scripts
├── .gitignore                          # Git ignore rules
├── Models/
│   ├── PackageInfo.cs                  # Package metadata model
│   └── AssemblyComparison.cs           # Comparison result models
└── Services/
    ├── PackageExtractor.cs             # NuGet package download & extraction
    ├── AssemblyAnalyzer.cs             # Reflection-based assembly comparison
    └── ComparisonReporter.cs           # Report generation (text/markdown/JSON)
```

## Key Features

### 1. Package Extraction
- Downloads packages from NuGet.org or custom sources
- Supports local .nupkg files
- Automatically extracts DLLs from lib/ folders
- Temporary directory management with cleanup

### 2. Assembly Analysis
- Uses .NET Reflection to analyze assemblies
- Compares all public types (classes, interfaces, enums, structs, delegates)
- Analyzes members (methods, properties, fields, events, constructors)
- Generates detailed signatures for comparison

### 3. Gap Analysis
- **Identical Types**: No changes detected
- **Modified Types**: Changes in members or signatures
- **Only in Package 1**: Types removed (potential breaking changes)
- **Only in Package 2**: New types added
- **Member Differences**:
  - Added members
  - Removed members
  - Signature changes
  - Accessibility changes

### 4. Report Formats
- **Text**: Console-friendly with Unicode box drawing
- **Markdown**: GitHub-compatible documentation format
- **JSON**: Machine-readable for automation/CI/CD

## Dependencies

- **NuGet.Packaging** (6.11.1): Package extraction
- **NuGet.Protocol** (6.11.1): Package download from feeds
- **System.CommandLine** (2.0.0-beta4): CLI argument parsing
- **System.Reflection.MetadataLoadContext** (8.0.0): Assembly reflection

## Usage Examples

### Basic Comparison
```powershell
dotnet run --project CanonicaLib.PackageComparer -- Newtonsoft.Json/13.0.1 Newtonsoft.Json/13.0.3
```

### Local .nupkg Files
```powershell
dotnet run --project CanonicaLib.PackageComparer -- package-v1.nupkg package-v2.nupkg
```

### Markdown Report with Details
```powershell
dotnet run --project CanonicaLib.PackageComparer -- MyPackage/1.0.0 MyPackage/2.0.0 -f markdown -v -o report.md
```

### JSON for Automation
```powershell
dotnet run --project CanonicaLib.PackageComparer -- Package1/1.0 Package2/1.0 -f json -o analysis.json
```

### Custom NuGet Source
```powershell
dotnet run --project CanonicaLib.PackageComparer -- MyPackage/1.0 MyPackage/2.0 -s https://my-feed.com/v3/index.json
```

## Command-Line Arguments

| Argument | Required | Description |
|----------|----------|-------------|
| package1 | Yes | First package (format: `PackageId/Version` or path to .nupkg) |
| package2 | Yes | Second package (format: `PackageId/Version` or path to .nupkg) |
| -o, --output | No | Output file path (defaults to console) |
| -f, --format | No | Output format: text, markdown, json (default: text) |
| -v, --verbose | No | Show detailed member-level changes |
| -s, --source | No | Custom NuGet source URL (default: nuget.org) |

## Report Sections

1. **Header**: Package identifiers and versions
2. **Summary**: Statistics (total, identical, modified, added, removed types)
3. **Assemblies**: List of DLLs in each package with version info
4. **Types Removed**: Breaking changes (only in package 1)
5. **Types Added**: New functionality (only in package 2)
6. **Types Modified**: Member-level changes with details
7. **Conclusion**: Overall compatibility assessment

## Use Cases

1. **API Compatibility Analysis**: Detect breaking changes between versions
2. **Migration Planning**: Identify gaps when upgrading dependencies
3. **Package Comparison**: Compare functionality of competing packages
4. **CI/CD Integration**: Automated breaking change detection
5. **Documentation**: Generate changelogs from package differences

## Technical Implementation

### PackageExtractor Service
- Downloads packages using NuGet.Protocol
- Extracts using NuGet.Packaging.PackageArchiveReader
- Manages temporary extraction directories
- Filters for .dll files in lib/ folder

### AssemblyAnalyzer Service
- Loads assemblies using Assembly.LoadFrom
- Enumerates GetExportedTypes() for public API
- Uses BindingFlags for granular member analysis
- Generates normalized signatures for comparison
- Creates diff list with categorized changes

### ComparisonReporter Service
- Template-based report generation
- Supports multiple output formats
- Verbose mode for detailed member info
- JSON serialization for automation

## Building & Publishing

### Development Build
```powershell
dotnet build CanonicaLib.PackageComparer
```

### Release Build
```powershell
dotnet build CanonicaLib.PackageComparer -c Release
```

### Publish as Executable
```powershell
dotnet publish CanonicaLib.PackageComparer -c Release -o ./publish
```

### Publish as Single-File Executable
```powershell
dotnet publish CanonicaLib.PackageComparer -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
```

## Integration with Solution

The project has been added to the canonicalib.sln solution file and is part of the build process. It can be built alongside the main CanonicaLib projects.

## Future Enhancements

Potential improvements:
- Support for analyzing XML documentation differences
- Attribute comparison
- Dependency tree analysis
- HTML report generation
- Semantic versioning recommendations
- Configuration file support
- Parallel assembly analysis for performance
- Support for analyzing multiple target frameworks
