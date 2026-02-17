# Coding Conventions

**Analysis Date:** 2026-02-17

## Naming Patterns

**Files:**
- PascalCase for all C# files: `DefaultSchemaGenerator.cs`, `StringExtensions.cs`
- Test files follow same pattern: `DefaultSchemaGeneratorTests.cs`
- Exception: Constants class uses PascalCase: `Methods.cs`

**Classes and Records:**
- PascalCase: `DefaultDiscoveryService`, `CanonicaLibOptions`, `GeneratorContext`
- Sealed where appropriate: `sealed record CanonicaLibOptions`, `sealed class DefaultParametersGenerator`
- Suffixes used: `*Service` (services), `*Generator` (generators), `*Handler` (handlers), `*Attribute` (attributes), `*Extensions` (extension methods)

**Methods and Properties:**
- PascalCase for public methods: `GenerateSchema()`, `FindCanonicalAssemblies()`
- PascalCase for public properties: `PageTitle`, `RootPath`, `Assembly`
- Private/internal members use camelCase with underscore prefix: `_discoveryService`, `_mockLogger`, `_schemaGenerator`
- Static helper methods use PascalCase: `GetSchemaKey()`, `IsNullableType()`

**Variables:**
- Local variables: camelCase: `schema`, `assembly`, `requiredProperties`
- Constants: UPPER_SNAKE_CASE in static classes: `MethodGet`, `MethodPost`
- Parameter names: camelCase: `endpointDefinition`, `generatorContext`

**Interfaces:**
- PascalCase prefixed with `I`: `ISchemaGenerator`, `IDiscoveryService`, `ILibrary`, `IExample`, `IService`, `ISecureService`

**Attributes:**
- PascalCase: `OpenApiPathAttribute`, `OpenApiEndpointAttribute`, `ExampleAttribute`
- Apply to parameters, classes, methods as appropriate

## Code Style

**Formatting:**
- Uses implicit usings (`.csproj` has `<ImplicitUsings>enable</ImplicitUsings>`)
- Target framework: .NET 8.0
- Nullable reference types enabled: `<Nullable>enable</Nullable>`
- Standard C# formatting with 4-space indentation

**Linting:**
- No explicit ESLint/Prettier equivalent detected
- Directory.Build.props configures MinVer for semantic versioning with tag prefix `v`
- Follows standard .NET/C# conventions

## Import Organization

**Order:**
1. System namespace imports (`using System;`, `using System.Reflection;`)
2. Microsoft/third-party imports (`using Microsoft.Extensions.Logging;`, `using Moq;`)
3. Project-specific imports (`using Zen.CanonicaLib.UI.Services;`)
4. Global usings (in GlobalUsings.cs): `global using Xunit;`

**Path Aliases:**
- Root namespace: `Zen.CanonicaLib` (consistent across all projects)
- Sub-namespaces organized by function: `.DataAnnotations`, `.UI`, `.UI.Services`, `.UI.Extensions`, `.UI.Handlers`
- Test project: `Zen.CanonicaLib.UI.Tests`

**Example from** `DefaultSchemaGeneratorTests.cs`:
```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Moq;
using System.Reflection;
using Xunit;
using Zen.CanonicaLib.UI.Services;
using Zen.CanonicaLib.UI.Services.Interfaces;
using Zen.CanonicaLib.UI.Tests.TestModels;
```

## Error Handling

**Patterns:**
- Explicit null checks with `ArgumentNullException`:
  ```csharp
  _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
  ```
- Throwing `InvalidOperationException` for invalid states:
  ```csharp
  throw new InvalidOperationException("No implementation of ILibrary found in the target assembly.");
  ```
- Wrapping exceptions with context:
  ```csharp
  throw new InvalidOperationException($"Failed to generate schema for type '{schemaDefinition.FullName}'", ex);
  ```
- Try-catch with logging before re-throw:
  ```csharp
  catch (Exception ex)
  {
      _logger.LogError(ex, "Failed to generate parameters for endpoint: {EndpointName}", endpointDefinition.Name);
      throw new InvalidOperationException($"Failed to generate parameters for endpoint '{endpointDefinition.Name}'", ex);
  }
  ```
- Graceful fallback in some scenarios (continue processing despite individual failures):
  ```csharp
  catch (Exception ex)
  {
      _logger.LogWarning(ex, "Failed to generate parameter: {ParameterName}", endpointParameter.Name);
      // Continue with other parameters instead of failing completely
  }
  ```

## Logging

**Framework:** `Microsoft.Extensions.Logging`

**Patterns:**
- Constructor injection: `ILogger<T>` injected as generic logger
- Use structured logging with named placeholders:
  ```csharp
  _logger.LogInformation("Creating schema for type: {TypeName}", type.FullName);
  _logger.LogError(ex, "Failed to generate parameters for endpoint: {EndpointName}", endpointDefinition.Name);
  ```
- Log levels used: `LogInformation()`, `LogDebug()`, `LogWarning()`, `LogError()`, `LogTrace()`
- Exception logging includes both the exception object and context message

## Comments

**When to Comment:**
- XML documentation comments (`///`) on public members: classes, methods, properties
- Inline comments for complex logic or non-obvious intent
- TODO comments for future improvements (found in code):
  ```csharp
  //TODO - this should also check base type recursively to see if it's an array
  // TODO extract the 'slug' from the inbound route parameters and convert it to an assembly name.
  ```

**JSDoc/TSDoc Style (C# XML Docs):**
- Used extensively on public APIs
- Include `<summary>`, `<param>`, `<returns>`, `<exception>`, `<remarks>`, `<value>`, `<example>` tags
- Example from `ExampleAttribute.cs`:
  ```csharp
  /// <summary>
  /// Specifies an example type to be used for documentation and schema generation.
  /// This attribute can be applied multiple times to provide different examples.
  /// </summary>
  /// <example>
  /// <code>
  /// public void ProcessData([Example(typeof(PersonExample))] Person person)
  /// {
  ///     // method implementation
  /// }
  /// </code>
  /// </example>
  ```

## Function Design

**Size:**
- Methods typically 20-80 lines
- Longer methods (100+ lines) broken into private helper methods
- Example: `DefaultSchemaGenerator.CreateSchemaFromType()` is 140 lines but contains 20+ helper method calls

**Parameters:**
- Maximum 3-4 parameters typically; use objects/records for more complex parameter groups
- Example: `GenerateSchema(Type schemaDefinition, GeneratorContext generatorContext)` - 2 params
- Constructor injection preferred over method parameters for services

**Return Values:**
- Nullable return types used explicitly: `IOpenApiSchema?`, `Assembly?`, `IList<Type>`
- Return null for "not found" scenarios rather than throwing
- Return empty collections (`new List<T>()`) rather than null for collections in some cases

## Module Design

**Exports:**
- Public classes explicitly public, interfaces explicitly public
- Services implement interfaces: `DefaultDiscoveryService : IDiscoveryService`
- Internal helper classes/extensions marked as `internal`:
  ```csharp
  internal static class StringExtensions
  internal static string? IfEmpty(this string input, string? defaultValue)
  ```

**Barrel Files:**
- Not explicitly used; namespace organization provides adequate organization
- Service interfaces in `Services.Interfaces` subdirectory
- Extensions grouped in `Extensions` directory

**Records:**
- Used for configuration objects: `public sealed record CanonicaLibOptions`
- Provides immutability and clear intent
- Used with init properties for read-only initialization:
  ```csharp
  public string RootPath { get; init; } = "/canonicalib";
  ```

**Validation:**
- Data annotations in configuration records:
  ```csharp
  [Required]
  [StringLength(200, MinimumLength = 1)]
  public string PageTitle { get; set; } = "CanonicaLib Assemblies";
  ```

---

*Convention analysis: 2026-02-17*
