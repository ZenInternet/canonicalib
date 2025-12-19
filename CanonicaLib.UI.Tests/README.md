# CanonicaLib.UI.Tests

Unit tests for the CanonicaLib.UI library, focusing on schema generation and OpenAPI document creation.

## Test Structure

### Services/DefaultSchemaGeneratorTests.cs

Comprehensive tests for the `DefaultSchemaGenerator` class, which handles OpenAPI schema generation from .NET types using reflection.

#### Key Test Scenarios

1. **Self-Referencing Types**
   - Tests that the schema generator correctly handles types that reference themselves (e.g., `Entity` with `Children` property of type `Entity[]`)
   - Verifies that infinite recursion is prevented by proper caching

2. **Circular References**
   - Tests types that reference each other (e.g., `Node` ↔ `Edge`)
   - Ensures both types are properly added to schema components

3. **Tree Structures**
   - Tests types with multiple self-references (e.g., `TreeNode` with `Left` and `Right` properties)
   - Validates that all properties correctly reference the same schema

4. **Simple Types**
   - Tests non-recursive types to ensure basic functionality works correctly
   - Verifies property detection, required fields, and schema structure

5. **Schema Reuse**
   - Tests that schemas are cached and reused when the same type is encountered multiple times
   - Ensures no duplicate schemas are created

6. **Primitive Types**
   - Tests handling of built-in types (string, int, DateTime, etc.)
   - Verifies they don't get added to schema components unnecessarily

7. **Required Properties**
   - Tests that value types are correctly marked as required
   - Verifies nullable reference types are not marked as required

8. **Error Handling**
   - Tests proper exception throwing for null arguments
   - Validates input parameter validation

## Test Models

### TestModels/SelfReferencingEntity.cs

Contains test models used across the test suite:

- `SelfReferencingEntity`: Entity with children array and parent reference
- `Node` & `Edge`: Circular reference between two types
- `TreeNode`: Binary tree structure with left/right references
- `SimpleEntity`: Non-recursive entity for baseline testing

## Running Tests

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test
dotnet test --filter "FullyQualifiedName~SelfReferencingEntity"

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Dependencies

- **xUnit**: Testing framework
- **Moq**: Mocking framework for dependencies
- **FluentAssertions**: Assertion library for readable test assertions

## Key Behaviors Verified

✅ **Infinite Recursion Prevention**: Schema generator adds schemas to context before processing properties, preventing stack overflow on self-referencing types

✅ **Schema Caching**: Once a schema is generated, subsequent requests return a reference rather than regenerating

✅ **Proper Schema References**: Self-referencing properties use `OpenApiSchemaReference` to point to the shared schema definition

✅ **Component Registration**: Complex types are properly registered in the document's `Components.Schemas` collection
