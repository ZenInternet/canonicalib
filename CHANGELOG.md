# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [0.11.0] - 2025-12-16

### Added
- **CanonicaLib.PackageComparer** - Comprehensive CLI tool for comparing NuGet packages and DLLs
  - Compare packages from NuGet.org, local `.nupkg` files, or DLL files directly
  - Automatic namespace change detection with member similarity matching
  - Attribute/annotation comparison support
  - Multiple output formats: text, markdown, JSON
  - AI-powered migration guide generation using ChatGPT (optional)
  - Secure API key configuration via `appsettings.Development.json`

### Fixed
- Preserve prerelease version suffixes (e.g., `-prerelease`, `-rc`, `-beta`) in OpenAPI version field by using `AssemblyInformationalVersionAttribute`

## [0.10.0] - (Previous Release)

### Added
- Initial versioned release
