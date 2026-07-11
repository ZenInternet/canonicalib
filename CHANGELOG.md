# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Root-level (global) document `security` support via `ISecureService.Security` (optional; defaults to none), surfaced through `ISecurityGenerator.GenerateDocumentSecurityRequirements` and written to `OpenApiDocument.Security`.
- `ISecurityGenerator.ValidateSecurity` — warns during generation when an operation references an undeclared security scheme or a scope not defined by that scheme's OAuth2 flows.
- First security-focused test coverage (`DefaultSecurityGeneratorTests`), including a regression guard that the operation security requirement serialises the scheme reference (not `{}`).

### Fixed
- Operation security requirements now serialise correctly. `OpenApiSecuritySchemeReference` is constructed with the generator's document, so a `[OpenApiSecurity(...)]`-annotated operation renders `"security": [ { "oauth2": [ ... ] } ]` instead of an empty `"security": [ {} ]`.
- Removed a duplicate `GenerateSchemas` call in `DefaultDocumentGenerator`.

### Changed
- **Breaking:** `ISecurityGenerator.GenerateOperationSecurityRequirements` now takes `(MethodInfo endpointDefinition, GeneratorContext generatorContext)`; the context supplies the host document required to resolve scheme references. Implementers must update their signature.
- `OpenApiSecurityAttribute` is now `AllowMultiple = true`, allowing multiple security schemes per operation.

## [1.1.0] - 2025-12-16

### Changed
- Remove git commit hash from version numbers by setting `MinVerBuildMetadata` to `none` - versions now display cleanly (e.g., `0.11.0-prerelease` instead of `0.11.0-prerelease+<commit-hash>`)

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
