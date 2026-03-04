# Phase 01: Schema Generation Core Fixes - Research

**Researched:** 2026-02-17
**Domain:** C# Reflection, OpenAPI Schema Generation, Circular Reference Handling
**Confidence:** HIGH

## Summary

This phase fixes two critical bugs in CanonicaLib's OpenAPI schema generator: infinite recursion on self-referencing models (BUG-01) and incomplete collection type detection (BUG-04). The research confirms that the existing codebase already has a partial solution in place for circular references using early schema registration with `GeneratorContext.AddSchema()`, but the implementation has gaps that cause infinite recursion in some code paths. The collection detection bug at line ~290 of `DefaultSchemaGenerator.cs` only checks the immediate type, not base types recursively.

OpenAPI 3.x fully supports circular references through `$ref` with a critical requirement: self-referencing properties must NOT be marked as required. Modern tooling like Redocly handles circular references gracefully when properly structured. The fix involves using `OpenApiSchemaReference` to create `$ref` references back to schema definitions, which the codebase already does correctly in some paths.

For collection detection, C#'s reflection provides `Type.GetInterfaces()` which automatically traverses the inheritance hierarchy, and `IsAssignableFrom()` for checking interface implementation. The current code at lines 300-304 already has recursive base type checking logic but it's not applied consistently to all collection checks.

**Primary recommendation:** Fix the circular reference bug by ensuring ALL code paths that create object schemas add them to the context BEFORE processing properties (move schema registration earlier in all branches), and fix collection detection by applying the existing recursive base type check (lines 300-304) to the initial type check at line 291.

## Standard Stack

The implementation uses the existing stack already in place in CanonicaLib.UI.

### Core
| Library | Version | Purpose | Why Standard |
|---------|---------|---------|--------------|
| Microsoft.OpenApi | 2.3.9+ | OpenAPI document generation | Official Microsoft OpenAPI library for .NET, industry standard for OpenAPI 3.x document creation |
| Namotion.Reflection | 3.4.3+ | XML documentation and nullability reflection | Advanced reflection APIs for C# 8+ nullable reference types and XML docs, used by NSwag/NJsonSchema |
| .NET 8 | 8.0 | Runtime and reflection APIs | Current LTS version with mature reflection APIs including `NullabilityInfoContext` |
| C# 12 | 12.0 | Language with nullable reference types | Provides nullable reference type annotations that schema generator must respect |

### Supporting
| Library | Version | Purpose | When to Use |
|---------|---------|---------|-------------|
| System.Reflection | Built-in | Type inspection and interface checking | All type analysis including `GetInterfaces()`, `IsAssignableFrom()`, base type traversal |
| xUnit | 2.6.2 | Test runner | Unit tests for schema generation scenarios |
| Moq | 4.20.70 | Mocking framework | Mock `IDiscoveryService` and `ILogger<T>` dependencies |
| FluentAssertions | 6.12.0 | Assertion library | Readable test assertions for schema validation |

### Alternatives Considered
| Instead of | Could Use | Tradeoff |
|------------|-----------|----------|
| Microsoft.OpenApi | Swashbuckle | Swashbuckle is ASP.NET Core specific; Microsoft.OpenApi is lower-level and more flexible |
| Namotion.Reflection | Manual NullabilityInfoContext | Namotion provides caching and convenience methods, mature library used by NSwag |
| xUnit | NUnit | xUnit is already in use, changing framework adds no value |

**Installation:**
Already installed. No additional packages needed.

## Architecture Patterns

### Recommended Approach: Early Schema Registration

The existing codebase already implements the correct pattern but inconsistently:

```csharp
// CORRECT PATTERN (lines 149-151 in DefaultSchemaGenerator.cs)
// Add schema to context BEFORE processing properties
var schemaAddedEarly = generatorContext.AddSchema(type, schema, referenceType);

// Process properties (which may reference the same type)
foreach (var property in properties)
{
    // Property processing can now safely reference the parent type
    schema.Properties[propertyName] = CreateSchemaFromType(property.PropertyType, propertyReferenceType, generatorContext);
}

// Return reference if schema was added
if (schemaAddedEarly)
{
    return new OpenApiSchemaReference(GetSchemaKey(type));
}
```

**How it works:**
1. Create empty `OpenApiSchema` object
2. Immediately add to `GeneratorContext.Schemas` dictionary BEFORE processing properties
3. When processing properties encounters the same type, `GetExistingSchema()` returns a reference
4. Return `OpenApiSchemaReference` to the schema key, not the schema object itself

**Current bug locations:**
- Lines 60-64: Early return for existing schemas works correctly
- Lines 134-139: Enum handling adds schema but doesn't consistently happen before property processing in all branches
- Lines 149-193: Object handling has correct pattern BUT multiple return paths may bypass it
- Lines 104-114: Array/collection handling doesn't add schemas early, causing infinite recursion when element type is self-referencing

### Pattern 1: Recursive Base Type Checking for Collections

**What:** Check if a type implements `IEnumerable<T>` by recursively examining base types
**When to use:** Detecting collection types including custom classes that inherit from `List<T>`, `Collection<T>`, etc.
**Current implementation:** Lines 300-304 already have this pattern for base type recursion

```csharp
// Source: DefaultSchemaGenerator.cs lines 288-305
private static bool IsArrayOrCollection(Type type)
{
    // BUG: Only checks immediate type (line 291-299)
    if (type.IsArray ||
          (type.IsGenericType &&
            (type.GetGenericTypeDefinition() == typeof(List<>) ||
             type.GetGenericTypeDefinition() == typeof(IList<>) ||
             type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
             type.GetGenericTypeDefinition() == typeof(Collection<>) ||
             type.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
    {
        return true;
    }
    else if (type.BaseType != null)  // Lines 300-304: Correct recursion
    {
        return IsArrayOrCollection(type.BaseType);
    }
    return false;
}
```

**Better approach using GetInterfaces():**
```csharp
// Recommended: Check all implemented interfaces (includes inherited ones)
private static bool IsArrayOrCollection(Type type)
{
    if (type.IsArray)
        return true;

    // GetInterfaces() returns ALL interfaces including inherited ones
    return type.GetInterfaces()
        .Any(i => i.IsGenericType &&
                  i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
}
```

**Source:** [Microsoft Learn - Type.GetInterfaces()](https://learn.microsoft.com/en-us/dotnet/api/system.type.getinterfaces?view=net-8.0)

### Pattern 2: Nullable Reference Type Handling

**What:** Respect C# nullability annotations when generating schema
**When to use:** All properties, especially self-referencing ones
**Implementation:** Already handled correctly by checking `IsRequiredProperty()` at lines 166-169

```csharp
// Lines 333-346
private static bool IsRequiredProperty(PropertyInfo property)
{
    var propertyType = property.PropertyType;

    // Value types (except nullable) are typically required
    if (propertyType.IsValueType && !IsNullableType(propertyType))
    {
        return true;
    }

    // Reference types and nullable value types are not required by default
    return false;
}
```

**Enhancement opportunity:**
Use `NullabilityInfoContext` (built into .NET 6+) or leverage Namotion.Reflection's nullability APIs to detect nullable reference type annotations (`string?` vs `string`).

**Source:** [Microsoft Learn - NullabilityInfo Class](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.nullabilityinfo?view=net-8.0)

### Pattern 3: OpenApiSchemaReference for $ref

**What:** Use `OpenApiSchemaReference` to create `$ref` references in OpenAPI output
**When to use:** Any time a schema should reference another schema definition instead of inlining it
**Current usage:** Lines 60-64, 137, 184, 191 - already implemented correctly

```csharp
// Source: DefaultSchemaGenerator.cs lines 60-64
var existingRef = generatorContext.GetExistingSchema(type);
if (existingRef != null)
{
    _logger.LogInformation("Schema for type {TypeName} already exists. Using existing schema reference.", type.FullName);
    return existingRef;
}

// Source: GeneratorContext.cs lines 56-62
public IOpenApiSchema? GetExistingSchema(Type type)
{
    var schemaKey = type.FullName ?? type.Name;
    if (Document.Components!.Schemas!.ContainsKey(schemaKey))
        return new OpenApiSchemaReference(schemaKey);
    return null;
}
```

**OpenAPI output:**
```json
{
  "components": {
    "schemas": {
      "TreeNode": {
        "type": "object",
        "properties": {
          "value": { "type": "integer" },
          "left": { "$ref": "#/components/schemas/TreeNode" },
          "right": { "$ref": "#/components/schemas/TreeNode" }
        }
      }
    }
  }
}
```

**Source:** [Microsoft Learn - OpenApiSchemaReference Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.openapi.openapischemareference)

### Anti-Patterns to Avoid

- **Processing properties before adding schema to context:** Causes infinite recursion on self-referencing types. Always add schema to context BEFORE calling `CreateSchemaFromType()` on properties.
- **Returning schema object instead of reference:** When a schema is added to components, return `OpenApiSchemaReference`, not the schema itself. The reference will be resolved by OpenAPI tooling.
- **Checking only immediate type for collections:** Custom classes inheriting from `List<T>` will be missed. Always check interfaces or recurse through base types.
- **Hardcoding specific collection types:** Using `typeof(List<>)`, `typeof(IList<>)`, etc. is fragile. Better to check for `IEnumerable<T>` which all collections implement.

## Don't Hand-Roll

Problems that look simple but have existing solutions:

| Problem | Don't Build | Use Instead | Why |
|---------|-------------|-------------|-----|
| Check if type implements IEnumerable | Manual interface checking loop | `Type.GetInterfaces()` | Built-in method returns all interfaces including inherited ones, handles inheritance correctly |
| Detect nullable reference types | Parse C# compiler attributes manually | `NullabilityInfoContext` or Namotion.Reflection | Complex reflection involving compiler-generated attributes; libraries handle all edge cases |
| Circular reference detection | Build visited set, manual tracking | Early schema registration + `GetExistingSchema()` check | OpenAPI design pattern, already implemented in codebase at lines 60-64 |
| Schema key generation | Complex logic for generic types | `type.FullName ?? type.Name` (line 52-55) | Already implemented correctly, handles most scenarios |
| Dictionary/additionalProperties | Custom schema generation | OpenAPI `additionalProperties` field | Standard OpenAPI pattern for dictionaries |

**Key insight:** The codebase already has correct patterns for most of these problems. The bugs are integration issues (not all code paths use the correct patterns) rather than missing functionality.

## Common Pitfalls

### Pitfall 1: Order of Operations in Schema Generation
**What goes wrong:** Adding schema to context AFTER processing properties causes infinite recursion when a property references the parent type.
**Why it happens:** Intuitive to fully build the schema before storing it, but self-referencing types create dependency cycles.
**How to avoid:**
1. Create empty schema object
2. Add to context immediately (even with empty properties)
3. Process properties (can now safely reference parent type)
4. Return reference, not schema object

**Warning signs:**
- Stack overflow exceptions in `CreateSchemaFromType()`
- Infinite loops when processing self-referencing models
- Schema generation never completes for tree structures

**Code location:** Lines 104-114 (array/collection handling) currently bypass early schema registration.

### Pitfall 2: Incomplete Collection Type Detection
**What goes wrong:** Custom collection classes (e.g., `class MyList<T> : List<T>`) are not detected as collections, generating incorrect schema with object properties instead of array type.
**Why it happens:** Only checking immediate type definition ignores inheritance hierarchy.
**How to avoid:**
1. Use `GetInterfaces()` to check all interfaces
2. OR recurse through `BaseType` chain (already implemented at lines 300-304)
3. Apply to ALL collection checks, not just the fallback case

**Warning signs:**
- Collection properties generate `type: "object"` instead of `type: "array"`
- Custom collection classes with extra properties generate incorrect schemas
- Generic collection types are not recognized

**Code location:** Lines 288-299 check immediate type only; lines 300-304 have correct recursion but only as fallback.

### Pitfall 3: Confusion Between Schema and Reference
**What goes wrong:** Returning `OpenApiSchema` object when it should return `OpenApiSchemaReference`, causing schema duplication or missing references.
**Why it happens:** Two `IOpenApiSchema` implementations: `OpenApiSchema` (the actual schema) and `OpenApiSchemaReference` (a `$ref` pointer).
**How to avoid:**
- Return `OpenApiSchemaReference` when schema is in components
- Return `OpenApiSchema` for inline schemas (primitives, anonymous types)
- Check if schema was added to context; if yes, return reference

**Warning signs:**
- Schemas appear inline instead of as references
- Large schemas duplicated throughout the document
- Missing `$ref` fields in generated JSON

**Code location:** Lines 180-193 correctly return reference when `schemaAddedEarly` is true.

### Pitfall 4: Dictionary and additionalProperties Handling
**What goes wrong:** `Dictionary<TKey, TValue>` types may generate incorrect schemas or cause errors.
**Why it happens:** OpenAPI represents dictionaries using `additionalProperties`, not as array types. User decision in CONTEXT.md leaves dictionary handling to Claude's discretion.
**How to avoid:**
- Detect `IDictionary<TKey, TValue>` interface
- Generate schema with `type: "object"` and `additionalProperties: { schema for TValue }`
- Don't treat dictionaries as collections (they shouldn't generate array schemas)

**Warning signs:**
- Dictionary properties generate array schemas
- Key-value pairs not represented correctly
- Dictionary types cause errors during schema generation

**Code location:** Lines 224-236 have partial dictionary detection for `IDictionary`1` and `IDictionary`2` but only return object type without `additionalProperties`.

## Code Examples

Verified patterns from the codebase and recommended fixes:

### Fix 1: Early Schema Registration for Arrays

**Current code (lines 104-114):**
```csharp
// BUG: Processes element type without adding array schema to context first
if (IsArrayOrCollection(type))
{
    schema.Type = JsonSchemaType.Array;
    var elementType = GetElementType(type);
    _logger.LogInformation("Type {TypeName} is an array or collection of {elementType}. Creating array schema.", type.FullName, elementType?.FullName);
    if (elementType != null)
    {
        schema.Items = CreateSchemaFromType(elementType, referenceType, generatorContext);
    }
    return schema;
}
```

**Fixed code:**
```csharp
if (IsArrayOrCollection(type))
{
    schema.Type = JsonSchemaType.Array;
    var elementType = GetElementType(type);
    _logger.LogInformation("Type {TypeName} is an array or collection of {elementType}. Creating array schema.", type.FullName, elementType?.FullName);

    // Add schema early to prevent infinite recursion if elementType references parent type
    var schemaAddedEarly = generatorContext.AddSchema(type, schema, referenceType);

    if (elementType != null)
    {
        schema.Items = CreateSchemaFromType(elementType, referenceType, generatorContext);
    }

    // Return reference if schema was added to context
    if (schemaAddedEarly)
    {
        var schemaKey = GetSchemaKey(type);
        return new OpenApiSchemaReference(schemaKey);
    }

    return schema;
}
```

### Fix 2: Improved Collection Detection

**Current code (lines 288-305):**
```csharp
private static bool IsArrayOrCollection(Type type)
{
    // BUG: Only checks immediate type, misses inherited collections
    if (type.IsArray ||
          (type.IsGenericType &&
            (type.GetGenericTypeDefinition() == typeof(List<>) ||
             type.GetGenericTypeDefinition() == typeof(IList<>) ||
             type.GetGenericTypeDefinition() == typeof(ICollection<>) ||
             type.GetGenericTypeDefinition() == typeof(Collection<>) ||
             type.GetGenericTypeDefinition() == typeof(IEnumerable<>))))
    {
        return true;
    }
    else if (type.BaseType != null)  // Correct pattern but only used as fallback
    {
        return IsArrayOrCollection(type.BaseType);
    }
    return false;
}
```

**Recommended fix (using GetInterfaces):**
```csharp
private static bool IsArrayOrCollection(Type type)
{
    // Arrays are always collections
    if (type.IsArray)
        return true;

    // Check if type implements IEnumerable<T> (all generic collections do)
    // GetInterfaces() includes interfaces from base classes automatically
    return type.GetInterfaces()
        .Any(i => i.IsGenericType &&
                  i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
}
```

**Alternative fix (keeping recursive pattern):**
```csharp
private static bool IsArrayOrCollection(Type type)
{
    if (type.IsArray)
        return true;

    // Check interfaces on current type
    if (type.IsGenericType &&
        type.GetInterfaces().Any(i => i.IsGenericType &&
                                     i.GetGenericTypeDefinition() == typeof(IEnumerable<>)))
        return true;

    // Recurse to base type
    if (type.BaseType != null && type.BaseType != typeof(object))
        return IsArrayOrCollection(type.BaseType);

    return false;
}
```

### Example 3: Dictionary Handling (Claude's Discretion)

**Current code (lines 224-236):**
```csharp
private static bool IsObjectType(Type type)
{
    var objectTypes = new[]
    {
        "IDictionary`1",  // Single-type parameter dictionary
        "IDictionary`2"   // Key-value dictionary
    };

    if (objectTypes.Contains(type.Name))
        return true;

    return false;
}
```

**Recommended enhancement:**
```csharp
private IOpenApiSchema CreateSchemaFromType(Type type, AssemblyReferenceType referenceType, GeneratorContext generatorContext)
{
    // ... existing code ...

    // Handle dictionaries BEFORE collection check (dictionaries implement IEnumerable but need different schema)
    if (IsDictionary(type))
    {
        schema.Type = JsonSchemaType.Object;

        // Extract value type from IDictionary<TKey, TValue>
        var dictionaryInterface = type.GetInterfaces()
            .FirstOrDefault(i => i.IsGenericType &&
                                 i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

        if (dictionaryInterface != null)
        {
            var valueType = dictionaryInterface.GetGenericArguments()[1]; // TValue
            schema.AdditionalProperties = CreateSchemaFromType(valueType, referenceType, generatorContext);
        }
        else
        {
            // Non-generic dictionary or IDictionary
            schema.AdditionalProperties = new OpenApiSchema { Type = JsonSchemaType.Object };
        }

        return schema;
    }

    // ... rest of existing code ...
}

private static bool IsDictionary(Type type)
{
    if (type.IsGenericType)
    {
        var genericTypeDef = type.GetGenericTypeDefinition();
        if (genericTypeDef == typeof(Dictionary<,>) ||
            genericTypeDef == typeof(IDictionary<,>))
            return true;
    }

    // Check if implements IDictionary<TKey, TValue>
    return type.GetInterfaces()
        .Any(i => i.IsGenericType &&
                  i.GetGenericTypeDefinition() == typeof(IDictionary<,>));
}
```

### Example 4: Test Pattern for Self-Referencing Types

**Current test (lines 34-57):**
```csharp
[Fact]
public void GenerateSchema_ShouldHandleSelfReferencingEntity_WithoutInfiniteRecursion()
{
    // Arrange
    var context = new GeneratorContext(_testAssembly);
    var entityType = typeof(SelfReferencingEntity);

    // Act
    var schema = _schemaGenerator.GenerateSchema(entityType, context);

    // Assert
    schema.Should().NotBeNull();
    schema.Should().BeOfType<OpenApiSchemaReference>();

    // Verify the schema was added to context
    var schemaKey = entityType.FullName ?? entityType.Name;
    context.Document.Components!.Schemas.Should().ContainKey(schemaKey);

    // Verify the actual schema has the self-referencing property
    var actualSchema = context.Document.Components.Schemas[schemaKey];
    actualSchema.Should().BeOfType<OpenApiSchema>();
    var openApiSchema = (OpenApiSchema)actualSchema;
    openApiSchema.Properties.Should().ContainKey("children");
    openApiSchema.Properties.Should().ContainKey("parent");
}
```

**Recommended additional test for BUG-04:**
```csharp
[Fact]
public void GenerateSchema_ShouldDetectCustomCollectionType()
{
    // Arrange - Custom class inheriting from List<T>
    var context = new GeneratorContext(_testAssembly);
    var customCollectionType = typeof(CustomList);

    // Act
    var schema = _schemaGenerator.GenerateSchema(customCollectionType, context);

    // Assert - Should be detected as array, not object
    schema.Should().NotBeNull();
    schema.Should().BeOfType<OpenApiSchemaReference>();

    var schemaKey = customCollectionType.FullName ?? customCollectionType.Name;
    var actualSchema = (OpenApiSchema)context.Document.Components.Schemas[schemaKey];

    actualSchema.Type.Should().Be(JsonSchemaType.Array);
    actualSchema.Items.Should().NotBeNull();
}

// Test model
public class CustomList : List<string>
{
    public string CustomProperty { get; set; } = string.Empty;
}
```

## State of the Art

| Old Approach | Current Approach | When Changed | Impact |
|--------------|------------------|--------------|--------|
| Manual visited set tracking | Early schema registration in context | Already in codebase | Prevents infinite recursion, aligns with OpenAPI $ref pattern |
| String concatenation for schema keys | `type.FullName ?? type.Name` | Already implemented (line 52-55) | Handles generic types correctly |
| Hardcoded collection type checks | Interface checking with `GetInterfaces()` | Industry standard, not yet in codebase | Handles custom collections and inheritance |
| Nullable annotation ignored | Namotion.Reflection nullability APIs | Library available but not fully utilized | Correctly marks optional vs required properties |
| OpenAPI 2.0 circular handling | OpenAPI 3.x $ref support | OpenAPI 3.0 (2017) | Native circular reference support in spec |

**Deprecated/outdated:**
- **OpenAPI 2.0 allOf workarounds:** OpenAPI 3.x has native circular reference support through $ref
- **Manual nullable detection via attributes:** Use `NullabilityInfoContext` (.NET 6+) or Namotion.Reflection instead
- **Type.IsSubclassOf() for interface checking:** Use `IsAssignableFrom()` or `GetInterfaces()` for more accurate results

## Open Questions

Things that couldn't be fully resolved:

1. **Custom Collection Classes with Additional Properties**
   - What we know: Custom classes inheriting from `List<T>` with extra properties need special handling
   - What's unclear: Should these generate array schemas (ignoring custom properties) or object schemas with both array items and properties?
   - Recommendation: Per CONTEXT.md, this is marked as "Claude's discretion". Recommend treating as arrays (follow inheritance semantics) but adding a note in changelog that custom properties on collection classes are not reflected in schema. If properties are important, users should use composition instead of inheritance.

2. **Dictionary Key Type Validation**
   - What we know: OpenAPI `additionalProperties` represents dictionary value schemas but doesn't validate key types
   - What's unclear: Should non-string dictionary keys generate warnings or be converted?
   - Recommendation: Follow OpenAPI convention where all object keys are strings. Emit warning log if dictionary key type is not string, but generate schema as if keys are strings (OpenAPI limitation).

3. **Nullability Detection Depth**
   - What we know: Current code uses simple value type check for required properties (lines 333-346)
   - What's unclear: Should nullable reference type annotations (`string?` vs `string`) be fully reflected in schema?
   - Recommendation: Enhancement for future phase. Current approach (value types required, reference types optional) is safe default. Full nullable reference type support would require Namotion.Reflection integration or `NullabilityInfoContext` usage.

4. **Schema Reference Naming Conflicts**
   - What we know: Schema keys use `type.FullName ?? type.Name` (line 52-55)
   - What's unclear: Generic types from different assemblies with same name could collide
   - Recommendation: Current approach is sufficient for single-assembly scenarios (CanonicaLib's current use case). If multi-assembly conflicts occur, enhance to include assembly name in schema key.

## Sources

### Primary (HIGH confidence)
- [Microsoft Learn - OpenApiSchemaReference Class](https://learn.microsoft.com/en-us/dotnet/api/microsoft.openapi.openapischemareference) - OpenApiSchemaReference API documentation
- [Microsoft Learn - Type.GetInterfaces()](https://learn.microsoft.com/en-us/dotnet/api/system.type.getinterfaces?view=net-8.0) - Reflection API for interface checking
- [Microsoft Learn - Type.IsAssignableFrom()](https://learn.microsoft.com/en-us/dotnet/api/system.type.isassignablefrom?view=net-8.0) - Type compatibility checking
- [Microsoft Learn - NullabilityInfo Class](https://learn.microsoft.com/en-us/dotnet/api/system.reflection.nullabilityinfo?view=net-8.0) - Nullable reference type reflection
- [Microsoft Learn - Versioning and .NET libraries](https://learn.microsoft.com/en-us/dotnet/standard/library-guidance/versioning) - Semantic versioning guidance
- [GitHub - Namotion.Reflection](https://github.com/RicoSuter/Namotion.Reflection) - Namotion.Reflection library documentation
- [NuGet - Namotion.Reflection 3.4.3](https://www.nuget.org/packages/Namotion.Reflection/) - Package information

### Secondary (MEDIUM confidence)
- [Speakeasy - References ($ref) in OpenAPI best practices](https://www.speakeasy.com/openapi/references) - OpenAPI reference patterns
- [pb33f.io - Circular References in OpenAPI](https://pb33f.io/libopenapi/circular-references/) - Circular reference handling patterns
- [Stainless - OpenAPI $ref Examples and Common Errors](https://www.stainless.com/sdk-api-best-practices/practical-guide-to-openapi-ref-examples-and-common-errors) - Practical $ref usage
- [Redocly - How to use JSON references](https://redocly.com/learn/openapi/ref-guide) - $ref guide
- [APImatic - OpenAPI additionalProperties](https://www.apimatic.io/openapi/additionalproperties) - Dictionary/additionalProperties patterns
- [OpenAPI Specification v3.1.0](https://spec.openapis.org/oas/v3.1.0.html) - Official OpenAPI specification
- [Semantic Versioning 2.0.0](https://semver.org/) - Semantic versioning standard

### Tertiary (LOW confidence)
- Various web search results about OpenAPI circular references - General patterns confirmed by primary sources
- Stack Overflow discussions on C# reflection - Patterns verified against official Microsoft documentation

## Metadata

**Confidence breakdown:**
- Standard stack: HIGH - All libraries already in use, versions confirmed from .csproj files
- Architecture: HIGH - Existing code patterns examined directly in DefaultSchemaGenerator.cs and GeneratorContext.cs
- Pitfalls: HIGH - Bug locations identified at specific line numbers in codebase
- Collection detection: HIGH - Multiple authoritative sources on Type.GetInterfaces() and reflection patterns
- OpenAPI circular references: HIGH - Official OpenAPI spec and Microsoft.OpenApi documentation
- Dictionary handling: MEDIUM - OpenAPI patterns well-documented, but specific implementation is Claude's discretion
- Nullability detection: MEDIUM - NullabilityInfoContext and Namotion.Reflection capabilities documented, but integration approach is flexible

**Research date:** 2026-02-17
**Valid until:** 60 days (stable technology stack, .NET 8 LTS)

## Key Implementation Notes

**For the planner:**

1. **No new dependencies needed** - All fixes use existing libraries and .NET reflection APIs
2. **Line-specific fixes:**
   - Lines 104-114: Add early schema registration for arrays/collections
   - Lines 288-305: Apply recursive check earlier or use `GetInterfaces()` approach
   - Lines 224-236: Enhance dictionary detection to set `additionalProperties`
3. **Test models exist:** `SelfReferencingEntity`, `Node/Edge`, `TreeNode`, `SimpleEntity` already in `TestModels/` directory
4. **Version bump guidance:** Minor version bump (not patch) per CONTEXT.md decision and semantic versioning standards - schema output changes are significant
5. **Breaking change scope:** Schema output format changes for self-referencing types (now use $ref) and collection types (some types now correctly detected as arrays instead of objects)
