# Roadmap: CanonicaLib Robustness Improvement

## Overview

A five-phase robustness pass that fixes all known bugs in schema generation, PackageComparer, and discovery services, then adds comprehensive unit test coverage across all type patterns. This ensures CanonicaLib generates correct OpenAPI documents reliably without crashing, even with edge-case type structures like self-referencing trees and circular references.

## Phases

**Phase Numbering:**
- Integer phases (1, 2, 3): Planned milestone work
- Decimal phases (2.1, 2.2): Urgent insertions (marked with INSERTED)

Decimal phases appear between their surrounding integers in numeric order.

- [x] **Phase 1: Schema Generation Core Fixes** - Fix infinite recursion and incomplete array detection
- [ ] **Phase 2: Schema Generation Testing** - Comprehensive unit tests for all type patterns
- [ ] **Phase 3: PackageComparer Reliability** - Fix thread safety and disposal issues
- [ ] **Phase 4: Discovery & Handler Fixes** - Complete slug conversion and tag group discovery
- [ ] **Phase 5: Exception Logging Hardening** - Add logging to all silent catch blocks

## Phase Details

### Phase 1: Schema Generation Core Fixes
**Goal**: Schema generator handles all type patterns correctly without infinite recursion or incorrect classifications
**Depends on**: Nothing (first phase)
**Requirements**: BUG-01, BUG-04
**Success Criteria** (what must be TRUE):
  1. Schema generation completes successfully for self-referencing models (tree structures with Parent/Children properties) without stack overflow
  2. All collection and array types (including custom IEnumerable implementations) are correctly detected and rendered as array schemas
  3. Schema generator recursively checks base types when determining collection status, catching all IEnumerable derivatives
**Plans**: 1 plan

Plans:
- [x] 01-01-PLAN.md — Fix infinite recursion (early schema registration) and collection/dictionary type detection

### Phase 2: Schema Generation Testing
**Goal**: Comprehensive unit test coverage validates schema generation correctness across all type patterns and edge cases
**Depends on**: Phase 1
**Requirements**: TEST-01, TEST-02, TEST-03, TEST-04, TEST-05, TEST-06, TEST-07
**Success Criteria** (what must be TRUE):
  1. Unit tests exist and pass for primitive types (string, int, DateTime, etc.)
  2. Unit tests exist and pass for enum types (simple enums, flag enums)
  3. Unit tests exist and pass for nested object types (classes with property hierarchies)
  4. Unit tests exist and pass for collection/array types (arrays, List, IEnumerable, custom collections)
  5. Unit tests exist and pass for generic types (generic classes, generic constraints)
  6. Unit tests exist and pass for self-referencing and circular reference patterns (existing tests expanded)
  7. Unit tests exist and pass for edge cases (nullable types, empty classes, deep nesting limits)
**Plans**: TBD

Plans:
- [ ] TBD

### Phase 3: PackageComparer Reliability
**Goal**: PackageComparer handles concurrent access safely and disposes all resources correctly under error conditions
**Depends on**: Phase 2
**Requirements**: BUG-02, BUG-06
**Success Criteria** (what must be TRUE):
  1. Concurrent calls to PackageExtractor.ExtractPackageAsync() initialize credentials safely without race conditions
  2. MetadataLoadContext is disposed under all code paths, including exceptions, preventing resource leaks
  3. Package extraction completes successfully under concurrent load without credential initialization failures
**Plans**: TBD

Plans:
- [ ] TBD

### Phase 4: Discovery & Handler Fixes
**Goal**: Assembly discovery and endpoint handlers correctly handle all input patterns and complete incomplete feature implementations
**Depends on**: Phase 3
**Requirements**: BUG-03, BUG-05
**Success Criteria** (what must be TRUE):
  1. Assembly slug-to-name conversion correctly resolves multi-level names (e.g., /my/assembly-name converts to My.AssemblyName)
  2. Tag group discovery automatically includes ungrouped tags in OpenAPI spec output
  3. Endpoint routing resolves assemblies correctly with multi-part namespace names
**Plans**: TBD

Plans:
- [ ] TBD

### Phase 5: Exception Logging Hardening
**Goal**: All exception catch blocks log errors with context, eliminating silent failures across all components
**Depends on**: Phase 4
**Requirements**: BUG-07
**Success Criteria** (what must be TRUE):
  1. All catch blocks in CanonicaLib.UI that previously swallowed exceptions now log error details via ILogger
  2. All catch blocks in CanonicaLib.PackageComparer that previously swallowed exceptions now log error details
  3. Assembly loading failures, reflection errors, and resource loading errors produce clear diagnostic log entries
  4. Developers can diagnose configuration and usage issues from log output alone
**Plans**: TBD

Plans:
- [ ] TBD

## Progress

**Execution Order:**
Phases execute in numeric order: 1 → 2 → 3 → 4 → 5

| Phase | Plans Complete | Status | Completed |
|-------|----------------|--------|-----------|
| 1. Schema Generation Core Fixes | 1/1 | ✓ Complete | 2026-02-17 |
| 2. Schema Generation Testing | 0/0 | Not started | - |
| 3. PackageComparer Reliability | 0/0 | Not started | - |
| 4. Discovery & Handler Fixes | 0/0 | Not started | - |
| 5. Exception Logging Hardening | 0/0 | Not started | - |

---
*Roadmap created: 2026-02-17*
