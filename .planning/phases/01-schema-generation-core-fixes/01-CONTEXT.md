# Phase 1: Schema Generation Core Fixes - Context

**Gathered:** 2026-02-17
**Status:** Ready for planning

<domain>
## Phase Boundary

Fix the schema generator to correctly handle self-referencing models (BUG-01) and collection/array type detection (BUG-04). These two bugs cause infinite recursion on tree structures and incorrect schema output for collection types that inherit from base classes.

</domain>

<decisions>
## Implementation Decisions

### Self-reference output
- Use `$ref` back to the same schema definition for self-referencing properties (e.g., TreeNode.Children references TreeNode)
- Mutual references (Node ↔ Edge) also use `$ref` both ways — each type gets its own schema definition
- No depth limit — rely purely on `$ref` to break all cycles at the schema level
- Respect C# nullability — if a self-referencing property is nullable (Parent?: TreeNode), the schema marks it as nullable

### Collection detection scope
- Claude's discretion on which types count as collections — pick what makes sense for correct OpenAPI output
- Claude's discretion on Dictionary handling — follow OpenAPI conventions
- Claude's discretion on custom classes inheriting from List<T> with extra properties

### Breaking changes
- Clean break — fix the bugs, bump minor version. No backward compatibility flag.
- Detailed changelog/release notes listing affected type patterns with before/after examples
- Minor version bump (not patch) since schema output changes are significant

### Claude's Discretion
- Exact collection detection heuristics (which IEnumerable derivatives to recognize)
- Dictionary<TKey, TValue> schema representation
- Custom collection class handling (inherits from List<T> with extra properties)
- Internal implementation approach for breaking circular references

</decisions>

<specifics>
## Specific Ideas

- Self-referencing models must produce valid OpenAPI that tools like Redocly can render without infinite expansion
- The fix should handle all patterns already in the test models: SelfReferencingEntity, Node/Edge circular, TreeNode

</specifics>

<deferred>
## Deferred Ideas

None — discussion stayed within phase scope

</deferred>

---

*Phase: 01-schema-generation-core-fixes*
*Context gathered: 2026-02-17*
