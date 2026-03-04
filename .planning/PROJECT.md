# CanonicaLib Robustness Improvement

## What This Is

A comprehensive robustness pass across all CanonicaLib components — CanonicaLib.UI (OpenAPI generation engine), CanonicaLib.DataAnnotations (contract definitions), and CanonicaLib.PackageComparer (CLI comparison tool). Fixing known bugs, adding test coverage, hardening error handling, and improving logging so the library is production-ready.

## Core Value

The library generates correct OpenAPI documents reliably without crashing, even with edge-case type structures like self-referencing trees and circular references.

## Requirements

### Validated

- ✓ Attribute-driven OpenAPI generation from C# interfaces — existing
- ✓ Schema generation via reflection (classes, enums, structs) — existing
- ✓ Partial circular reference detection in schema generation — existing (has tests, but incomplete)
- ✓ Assembly discovery and introspection — existing
- ✓ UI for browsing generated docs (Redocly integration) — existing
- ✓ Package comparison CLI tool with NuGet integration — existing
- ✓ MinVer-based versioning and NuGet publishing pipeline — existing
- ✓ Webhook and security scheme generation — existing

### Active

- [ ] Fix infinite recursion on self-referencing/tree-structure models in schema generation
- [ ] Fix race condition in PackageExtractor static credential initialization
- [ ] Fix silent exception swallowing — add logging to all catch blocks that currently discard errors
- [ ] Fix incomplete assembly slug-to-name conversion in AssemblyEndpointHandler
- [ ] Fix incomplete array/collection type detection in schema generator (recursive base type checking)
- [ ] Fix incomplete tag group discovery (auto-discover ungrouped tags)
- [ ] Fix MetadataLoadContext disposal issue in PackageExtractor
- [ ] Add comprehensive unit tests for schema generation (all type patterns, edge cases)
- [ ] Add unit tests for discovery service
- [ ] Add unit tests for endpoint handlers
- [ ] Add unit tests for PackageComparer services
- [ ] Harden error handling across all components — clear error messages, no silent failures
- [ ] Replace Console.WriteLine with ILogger in PackageComparer
- [ ] Add input validation for slug parameters in handlers

### Out of Scope

- Performance optimization (reflection caching, schema caching, response compression) — separate effort, not blocking production readiness
- New features or API surface changes — this is a stability pass
- UI/view changes — Razor views and frontend not in scope
- .NET version upgrade — staying on .NET 8
- Integration or E2E tests — focus on unit tests for this pass

## Context

CanonicaLib is a library that lets developers define canonical API contracts using C# attributes on interfaces and types, then generates OpenAPI documentation from those definitions via reflection. It's published to NuGet as three packages.

The codebase has a single test project (`CanonicaLib.UI.Tests`) focused on schema generation circular reference handling. Test coverage is thin — no tests for handlers, discovery service, or PackageComparer services. The codebase analysis identified a race condition, multiple silent exception catches, incomplete feature implementations (slug conversion, array detection, tag groups), and a known infinite recursion issue with self-referencing models that bypasses existing protections.

The project uses xUnit + FluentAssertions + Moq for testing, strict compiler settings (warnings as errors, nullable enabled), and Roslyn analyzers.

## Constraints

- **Tech stack**: C# / .NET 8 / xUnit / FluentAssertions / Moq — no new dependencies
- **Compatibility**: CanonicaLib.DataAnnotations targets netstandard2.1 — changes must not break compatibility
- **Build**: Warnings-as-errors is enforced — all code must compile clean
- **Testing**: Follow existing patterns (AAA, constructor setup, Moq for deps, FluentAssertions)

## Key Decisions

| Decision | Rationale | Outcome |
|----------|-----------|---------|
| Fix bugs before adding tests | Ensures tests validate correct behavior, not broken behavior | — Pending |
| Unit tests only (no integration/E2E) | Fastest path to coverage; integration tests are a separate effort | — Pending |
| All three components in scope | User wants production-ready across the board, not just the generation engine | — Pending |

---
*Last updated: 2026-02-17 after initialization*
