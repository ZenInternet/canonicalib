# Testing Patterns

**Analysis Date:** 2026-02-17

## Test Framework

**Runner:**
- xUnit 2.6.2
- Config: `CanonicaLib.UI.Tests/CanonicaLib.UI.Tests.csproj` (no separate config file - uses project defaults)

**Assertion Library:**
- FluentAssertions 6.12.0 (chainable, readable assertions)
- xUnit built-in assertions also used

**Test SDK:**
- Microsoft.NET.Test.Sdk 17.8.0
- Test runner: xunit.runner.visualstudio 2.5.4

**Coverage:**
- coverlet.collector 6.0.0 for code coverage collection

**Run Commands:**
```bash
dotnet test                              # Run all tests
dotnet test --watch                      # Watch mode
dotnet test /p:CollectCoverage=true      # Collect coverage
```

## Test File Organization

**Location:**
- Separate project: `CanonicaLib.UI.Tests/` (not co-located with source)
- Mirror structure of source: `CanonicaLib.UI.Tests/Services/` mirrors `CanonicaLib.UI/Services/`

**Naming:**
- Pattern: `[SourceClass]Tests.cs`
- Example: `DefaultSchemaGeneratorTests.cs` tests `DefaultSchemaGenerator.cs`
- Test model classes in `TestModels/` subdirectory

**Structure:**
```
CanonicaLib.UI.Tests/
├── GlobalUsings.cs              # Global using statements
├── Services/
│   └── DefaultSchemaGeneratorTests.cs
└── TestModels/
    └── SelfReferencingEntity.cs (contains all test entity models)
```

## Test Structure

**Suite Organization:**
```csharp
public class DefaultSchemaGeneratorTests
{
    // Fields for dependencies and SUT
    private readonly Mock<IDiscoveryService> _mockDiscoveryService;
    private readonly Mock<ILogger<DefaultSchemaGenerator>> _mockLogger;
    private readonly DefaultSchemaGenerator _schemaGenerator;
    private readonly Assembly _testAssembly;

    // Constructor for setup
    public DefaultSchemaGeneratorTests()
    {
        _mockDiscoveryService = new Mock<IDiscoveryService>();
        _mockLogger = new Mock<ILogger<DefaultSchemaGenerator>>();
        _schemaGenerator = new DefaultSchemaGenerator(_mockDiscoveryService.Object, _mockLogger.Object);
        _testAssembly = typeof(SelfReferencingEntity).Assembly;

        // Setup default behavior
        _mockDiscoveryService
            .Setup(x => x.GetAssemblyReferenceType(It.IsAny<Assembly>(), It.IsAny<Type>()))
            .Returns(AssemblyReferenceType.Internal);
    }

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
    }
}
```

**Patterns:**
- **Setup:** Constructor-based setup with field initialization
- **Teardown:** No explicit teardown needed (xUnit handles)
- **Assertion:** FluentAssertions with `.Should()` chain syntax
- **Test naming:** `[MethodName]_Should[ExpectedBehavior]_When[Condition]()`
- **Structure:** AAA pattern (Arrange, Act, Assert) with clear comment sections

## Mocking

**Framework:** Moq 4.20.70

**Patterns:**
```csharp
// Create mock
private readonly Mock<IDiscoveryService> _mockDiscoveryService = new Mock<IDiscoveryService>();

// Setup behavior
_mockDiscoveryService
    .Setup(x => x.GetAssemblyReferenceType(It.IsAny<Assembly>(), It.IsAny<Type>()))
    .Returns(AssemblyReferenceType.Internal);

// Access mock object
_schemaGenerator = new DefaultSchemaGenerator(_mockDiscoveryService.Object, _mockLogger.Object);

// Setup in test method
_mockDiscoveryService
    .Setup(x => x.GetAssemblyReferenceType(It.IsAny<Assembly>(), typeof(string)))
    .Returns(AssemblyReferenceType.Excluded);
```

**What to Mock:**
- External dependencies: `IDiscoveryService`, `ILogger<T>`
- Services that would require complex setup or external resources
- Anything that varies between test scenarios (return values, behavior)

**What NOT to Mock:**
- Value types and simple POCOs (use real instances)
- Configuration objects like `GeneratorContext`
- Test model classes (use real instances)
- Classes under test

## Fixtures and Factories

**Test Data:**
- Test models defined as actual classes in `TestModels/SelfReferencingEntity.cs`
- Models include: `SelfReferencingEntity`, `Node`, `Edge`, `TreeNode`, `SimpleEntity`
- Each demonstrates different reference pattern (self-reference, circular, tree structure, simple)

```csharp
public class SelfReferencingEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public SelfReferencingEntity[]? Children { get; set; }
    public SelfReferencingEntity? Parent { get; set; }
}

public class Node
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<Edge>? Edges { get; set; }
}
```

**Location:**
- `CanonicaLib.UI.Tests/TestModels/` directory
- Separate namespace: `Zen.CanonicaLib.UI.Tests.TestModels`
- Instantiated directly in test methods, not in shared fixtures

## Coverage

**Requirements:** No enforced coverage threshold detected

**View Coverage:**
```bash
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover
```

## Test Types

**Unit Tests:**
- Scope: Individual method/class behavior
- Approach: Constructor injection of mocks, test single responsibility
- Example: `GenerateSchema_ShouldHandleSelfReferencingEntity_WithoutInfiniteRecursion()` tests `DefaultSchemaGenerator.GenerateSchema()`
- Isolation: Full isolation with mocked dependencies

**Integration Tests:**
- Not detected in current codebase
- Could be added in separate project `CanonicaLib.UI.Integration.Tests` if needed

**E2E Tests:**
- Not used; focus is on unit testing service generation logic

## Common Patterns

**Fact vs Theory:**
- `[Fact]` used for single-scenario tests (majority of tests)
- `[Theory]` not detected in current codebase
- Good candidate for adding `[Theory]` with `[InlineData]` for testing multiple input scenarios

**Null Argument Testing:**
```csharp
[Fact]
public void GenerateSchema_ShouldThrowArgumentNullException_WhenSchemaDefinitionIsNull()
{
    // Arrange
    var context = new GeneratorContext(_testAssembly);

    // Act & Assert
    Assert.Throws<ArgumentNullException>(() =>
        _schemaGenerator.GenerateSchema(null!, context));
}
```

**Assertion Patterns:**
```csharp
// FluentAssertions
schema.Should().NotBeNull();
schema.Should().BeOfType<OpenApiSchemaReference>();

// Collection assertions
actualSchema.Properties.Should().ContainKey("children");
actualSchema.Properties.Should().HaveCount(3);

// Property assertions
actualSchema.Type.Should().Be(JsonSchemaType.Object);
actualSchema.Title.Should().Be(entityType.Name);

// xUnit assertions (less common but present)
Assert.Throws<ArgumentNullException>(() =>
    _schemaGenerator.GenerateSchema(null!, context));
```

**Context Management:**
```csharp
// Fresh context per test
var context = new GeneratorContext(_testAssembly);

// Pre-configured mocks reused in constructor setup
// Per-test additional setup when needed:
_mockDiscoveryService
    .Setup(x => x.GetAssemblyReferenceType(It.IsAny<Assembly>(), typeof(string)))
    .Returns(AssemblyReferenceType.Excluded);
```

## Test-Driven Development Indicators

**Observable patterns:**
- Comprehensive test coverage for circular reference handling (primary feature)
- Multiple reference pattern scenarios tested: self-reference, circular, tree, simple
- Edge cases covered: null arguments, multiple calls, primitive types
- Clear intent of each test through descriptive naming
- Tests verify both happy path and error conditions

**Areas for Improvement:**
- Consider `[Theory]` with `[InlineData]` for parametrized tests
- Add more integration tests for multi-assembly scenarios
- Test logging output verification (currently only mocked)

---

*Testing analysis: 2026-02-17*
