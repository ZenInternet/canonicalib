# PackageComparer Deployment Guide

The CanonicaLib.PackageComparer tool is distributed in multiple formats to maximize flexibility for end users.

## Distribution Methods

### 1. .NET Global Tool (NuGet)

**Best for:** Developers with .NET SDK installed

**Installation:**
```bash
dotnet tool install -g CanonicaLib.PackageComparer
```

**Update:**
```bash
dotnet tool update -g CanonicaLib.PackageComparer
```

**Usage:**
```bash
canonicalib-comparer Newtonsoft.Json/13.0.1 Newtonsoft.Json/13.0.3
```

**Advantages:**
- Easy installation via dotnet CLI
- Automatic updates available
- Cross-platform support
- Small package size (dependencies downloaded on-demand)

### 2. Standalone Executables (GitHub Releases)

**Best for:** Users without .NET SDK or in restricted environments

**Available Platforms:**
- Windows x64 (`canonicalib-comparer-win-x64.exe`)
- Linux x64 (`canonicalib-comparer-linux-x64`)
- macOS x64 (`canonicalib-comparer-osx-x64`)
- macOS ARM64 (Apple Silicon) (`canonicalib-comparer-osx-arm64`)

**Installation:**
1. Download from [GitHub Releases](https://github.com/ZenInternet/canonicalib/releases)
2. Make executable (Linux/macOS): `chmod +x canonicalib-comparer-*`
3. Optionally move to PATH location

**Advantages:**
- No .NET SDK required (self-contained)
- Single-file executable
- Works in air-gapped environments
- No internet connection needed after download

## Automated Release Process

When code is merged to `main`, the GitHub Actions workflow automatically:

1. **Builds standalone executables** for all platforms:
   - Windows x64
   - Linux x64
   - macOS x64 (Intel)
   - macOS ARM64 (Apple Silicon)

2. **Packages as .NET Global Tool** and publishes to NuGet.org

3. **Creates GitHub Release** with:
   - Version tag (e.g., `v1.2.0`)
   - Release notes
   - Attached standalone executables
   - Installation instructions

4. **Publishes all packages** to NuGet.org:
   - `CanonicaLib.DataAnnotations`
   - `CanonicaLib.UI`
   - `CanonicaLib.PackageComparer` (as .NET tool)

## Package Configuration

The PackageComparer is configured with:

```xml
<!-- .NET Global Tool settings -->
<PackAsTool>true</PackAsTool>
<ToolCommandName>canonicalib-comparer</ToolCommandName>

<!-- Standalone executable settings -->
<PublishSingleFile>true</PublishSingleFile>
<SelfContained>true</SelfContained>
<IncludeNativeLibrariesForSelfExtract>true</IncludeNativeLibrariesForSelfExtract>
```

## User Installation Decision Tree

```
Does user have .NET SDK installed?
├─ Yes → Use .NET Global Tool
│         ✓ Easiest to install and update
│         ✓ Smallest download size
│
└─ No → Download standalone executable
          ✓ No prerequisites needed
          ✓ Works offline
          └─ Choose platform:
              ├─ Windows → win-x64.exe
              ├─ Linux → linux-x64
              ├─ macOS Intel → osx-x64
              └─ macOS Apple Silicon → osx-arm64
```

## Version Management

All distribution methods receive the same version number (e.g., `1.2.0`):
- Synchronized via MinVer and GitHub Actions
- Same functionality across all platforms
- Version displayed via `--version` flag

## Future Distribution Options

Additional distribution methods that could be added:

### Chocolatey (Windows)
```powershell
choco install canonicalib-comparer
```

### Homebrew (macOS/Linux)
```bash
brew install canonicalib-comparer
```

### Winget (Windows)
```powershell
winget install ZenInternet.CanonicaLib.PackageComparer
```

### Snap (Linux)
```bash
snap install canonicalib-comparer
```

### Docker Image
```bash
docker run zeninternet/canonicalib-comparer Package1/1.0 Package2/2.0
```

## Testing Locally

### Test .NET Global Tool packaging:
```bash
dotnet pack CanonicaLib.PackageComparer -c Release
dotnet tool install -g CanonicaLib.PackageComparer --add-source ./bin/Release
```

### Test standalone executable build:
```bash
dotnet publish CanonicaLib.PackageComparer -c Release -r win-x64 --self-contained -p:PublishSingleFile=true -o ./publish
./publish/CanonicaLib.PackageComparer.exe --help
```

## CI/CD Pipeline

The release job in `.github/workflows/build-and-publish.yml` handles:

1. Version calculation from git tags
2. Building for multiple platforms
3. Creating GitHub release with binaries
4. Publishing to NuGet.org
5. Automatic tagging

See [RELEASE-PIPELINE.md](../.github/RELEASE-PIPELINE.md) for complete workflow documentation.
