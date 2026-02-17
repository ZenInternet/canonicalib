# Requirements: CanonicaLib Robustness Improvement

**Defined:** 2026-02-17
**Core Value:** The library generates correct OpenAPI documents reliably without crashing, even with edge-case type structures.

## v1 Requirements

### Bug Fixes

- [ ] **BUG-01**: Schema generation handles self-referencing and tree-structure models without infinite recursion
- [ ] **BUG-02**: PackageExtractor credential initialization is thread-safe under concurrent access
- [ ] **BUG-03**: Assembly slug-to-name conversion correctly resolves multi-level names (e.g., `/my/assembly-name` → `My.AssemblyName`)
- [ ] **BUG-04**: Schema generator detects array/collection types through recursive base type checking (all IEnumerable derivatives)
- [ ] **BUG-05**: Tag group discovery includes ungrouped tags automatically in the OpenAPI spec
- [ ] **BUG-06**: MetadataLoadContext is properly disposed under all error conditions in PackageExtractor
- [ ] **BUG-07**: All catch blocks that currently swallow exceptions silently now log the error via ILogger

### Testing

- [ ] **TEST-01**: Unit tests for schema generation with primitive types
- [ ] **TEST-02**: Unit tests for schema generation with enum types
- [ ] **TEST-03**: Unit tests for schema generation with nested object types
- [ ] **TEST-04**: Unit tests for schema generation with collection/array types
- [ ] **TEST-05**: Unit tests for schema generation with generic types
- [ ] **TEST-06**: Unit tests for schema generation with self-referencing and circular reference patterns
- [ ] **TEST-07**: Unit tests for schema generation edge cases (nullable types, empty classes, deep nesting)

## v2 Requirements

### Testing Expansion

- **TEST-V2-01**: Unit tests for discovery service (assembly finding, controller detection, schema detection)
- **TEST-V2-02**: Unit tests for endpoint handlers (UI, assemblies, redocly — happy path and errors)
- **TEST-V2-03**: Unit tests for PackageComparer services (analyzer, extractor, reporter)

### Error Handling Expansion

- **ERR-V2-01**: Replace Console.WriteLine with ILogger in PackageComparer (58 instances)
- **ERR-V2-02**: Validate slug input format in endpoint handlers before assembly lookup
- **ERR-V2-03**: User-facing errors explain what went wrong and suggest resolution

## Out of Scope

| Feature | Reason |
|---------|--------|
| Performance optimization (caching, reflection caching) | Separate effort — not blocking production readiness |
| New features or API surface changes | This is a stability pass, not feature work |
| UI/Razor view changes | Frontend not in scope for robustness work |
| .NET version upgrade | Staying on .NET 8 for this milestone |
| Integration or E2E tests | Focus on unit tests; integration tests are v2+ |

## Traceability

| Requirement | Phase | Status |
|-------------|-------|--------|
| BUG-01 | Phase 1 | Complete |
| BUG-04 | Phase 1 | Complete |
| TEST-01 | Phase 2 | Pending |
| TEST-02 | Phase 2 | Pending |
| TEST-03 | Phase 2 | Pending |
| TEST-04 | Phase 2 | Pending |
| TEST-05 | Phase 2 | Pending |
| TEST-06 | Phase 2 | Pending |
| TEST-07 | Phase 2 | Pending |
| BUG-02 | Phase 3 | Pending |
| BUG-06 | Phase 3 | Pending |
| BUG-03 | Phase 4 | Pending |
| BUG-05 | Phase 4 | Pending |
| BUG-07 | Phase 5 | Pending |

**Coverage:**
- v1 requirements: 14 total
- Mapped to phases: 14
- Unmapped: 0

---
*Requirements defined: 2026-02-17*
*Last updated: 2026-02-17 after roadmap creation*
