# Codebase Structure

**Analysis Date:** 2026-02-17

## Directory Layout

```
canonicalib/
├── CanonicaLib.DataAnnotations/        # Core attributes and contract interfaces (NuGet package)
│   ├── ExampleAttribute.cs
│   ├── IExample.cs
│   ├── ILibrary.cs
│   ├── IService.cs
│   ├── ISecureService.cs
│   ├── OpenApiEndpointAttribute.cs
│   ├── OpenApiExcludedTypeAttribute.cs
│   ├── OpenApiParameterAttribute.cs
│   ├── OpenApiPathAttribute.cs
│   ├── OpenApiSecurityAttribute.cs
│   ├── OpenApiTagAttribute.cs
│   ├── OpenApiTagGroup.cs
│   ├── OpenApiWebhookAttribute.cs
│   ├── ResponseAttribute.cs
│   ├── ResponseExampleAttribute.cs
│   ├── ResponseHeaderAttribute.cs
│   ├── FromRequest*.cs                 # Parameter binding attributes
│   ├── Methods.cs
│   └── CanonicaLib.DataAnnotations.csproj
│
├── CanonicaLib.UI/                     # OpenAPI generation and UI services (NuGet package)
│   ├── Extensions/
│   │   ├── AssemblyExtensions.cs       # Reflection helpers
│   │   ├── ExampleAttributeExtensions.cs
│   │   ├── StringExtensions.cs
│   ├── Handlers/                       # HTTP request handlers
│   │   ├── AssembliesEndpointHandler.cs
│   │   ├── AssemblyEndpointHandler.cs
│   │   ├── AttachmentEndpointHandler.cs
│   │   ├── RedoclyEndpointHandler.cs
│   │   └── UIEndpointHandler.cs
│   ├── Models/                         # View models
│   │   ├── AssembliesViewModel.cs
│   │   ├── AssemblyViewModel.cs
│   │   └── RedoclyViewModel.cs
│   ├── OpenApiExtensions/              # Custom OpenAPI extensions
│   │   ├── TagGroupsExtension.cs
│   │   ├── TagsExtension.cs
│   │   └── ValidationResultsExtension.cs
│   ├── Services/
│   │   ├── Interfaces/                 # Service contracts
│   │   │   ├── IDiscoveryService.cs
│   │   │   ├── IDocumentGenerator.cs
│   │   │   ├── IExamplesGenerator.cs
│   │   │   ├── IHeadersGenerator.cs
│   │   │   ├── IInfoGenerator.cs
│   │   │   ├── IOperationGenerator.cs
│   │   │   ├── IParametersGenerator.cs
│   │   │   ├── IPathsGenerator.cs
│   │   │   ├── IRequestBodyGenerator.cs
│   │   │   ├── IResponsesGenerator.cs
│   │   │   ├── ISchemaGenerator.cs
│   │   │   ├── ISchemasGenerator.cs
│   │   │   ├── ISecurityGenerator.cs
│   │   │   ├── IServersGenerator.cs
│   │   │   ├── ITagGroupsGenerator.cs
│   │   │   └── IWebhooksGenerator.cs
│   │   ├── DefaultDiscoveryService.cs
│   │   ├── DefaultDocumentGenerator.cs
│   │   ├── DefaultExamplesGenerator.cs
│   │   ├── DefaultHeadersGenerator.cs
│   │   ├── DefaultInfoGenerator.cs
│   │   ├── DefaultOperationGenerator.cs
│   │   ├── DefaultParametersGenerator.cs
│   │   ├── DefaultPathsGenerator.cs
│   │   ├── DefaultRequestBodyGenerator.cs
│   │   ├── DefaultResponsesGenerator.cs
│   │   ├── DefaultSchemaGenerator.cs
│   │   ├── DefaultSchemasGenerator.cs
│   │   ├── DefaultSecurityGenerator.cs
│   │   ├── DefaultServersGenerator.cs
│   │   ├── DefaultTagGroupsGenerator.cs
│   │   ├── DefaultWebhooksGenerator.cs
│   │   └── AssemblyReferenceType.cs    # Enum for internal/external/excluded classification
│   ├── Views/
│   │   ├── CanonicaLib/
│   │   │   ├── Index.cshtml            # Main UI landing page
│   │   │   └── Redocly.cshtml          # OpenAPI documentation viewer
│   │   ├── Shared/
│   │   │   └── _Layout.cshtml          # Master layout template
│   │   ├── _ViewImports.cshtml
│   │   └── _ViewStart.cshtml
│   ├── GeneratorContext.cs             # Shared context across generation pipeline
│   ├── CanonicaLibOptions.cs           # Configuration options and post-processor delegates
│   └── CanonicaLib.UI.csproj
│
├── CanonicaLib.UI.Tests/               # Unit tests for UI layer
│   ├── Services/
│   │   └── DefaultSchemaGeneratorTests.cs
│   ├── TestModels/
│   │   └── SelfReferencingEntity.cs
│   ├── GlobalUsings.cs
│   └── CanonicaLib.UI.Tests.csproj
│
├── CanonicaLib.PackageComparer/        # CLI tool for package comparison (published as .NET tool)
│   ├── Services/
│   │   ├── AssemblyAnalyzer.cs
│   │   ├── ComparisonReporter.cs
│   │   ├── MigrationGuideGenerator.cs
│   │   └── PackageExtractor.cs
│   ├── Models/
│   │   ├── AssemblyComparison.cs
│   │   └── PackageInfo.cs
│   ├── Program.cs
│   └── CanonicaLib.PackageComparer.csproj
│
├── Example/                            # Example and reference implementations
│   ├── Contract/                       # Example canonical contract definitions
│   │   ├── Controllers/
│   │   ├── Examples/
│   │   ├── Webhooks/
│   │   └── Contract.csproj
│   ├── DesignSpecifications/           # OpenAPI design specifications
│   │   ├── Properties/
│   │   └── DesignSpecifications.csproj
│
├── .planning/                          # GSD planning and analysis documents
│   └── codebase/
│       ├── ARCHITECTURE.md
│       ├── STRUCTURE.md
│       ├── CONVENTIONS.md
│       ├── TESTING.md
│       ├── STACK.md
│       ├── INTEGRATIONS.md
│       └── CONCERNS.md
│
├── .github/                            # GitHub workflows and configuration
│
├── .vscode/                            # VS Code settings
│
├── docs/                               # Documentation
│   └── GETTINGSTARTED.md
│
├── canonicalib.sln                     # Solution file
├── Directory.Build.props                # Shared build properties
├── README.md
├── CHANGELOG.md
├── CONTRIBUTING.md
├── LICENSE
└── .gitignore
```

## Directory Purposes

**CanonicaLib.DataAnnotations:**
- Purpose: Defines the contract language for canonical libraries
- Contains: Attributes for marking controllers, endpoints, parameters, responses, webhooks
- Key files: `ILibrary.cs`, `IService.cs`, `OpenApiEndpointAttribute.cs`, `FromRequest*.cs`
- Published as: NuGet package `Zen.CanonicaLib.DataAnnotations`

**CanonicaLib.UI:**
- Purpose: Generates OpenAPI documents and serves discovery UI
- Contains: Discovery service, generator services, HTTP handlers, Razor views
- Key subfolders:
  - `Services/` - The generation pipeline (16 generator implementations)
  - `Handlers/` - 5 HTTP request handlers for different routes
  - `Views/` - Razor templates for UI pages
  - `Extensions/` - Reflection and string utilities
  - `Models/` - View model DTOs
  - `OpenApiExtensions/` - Custom OpenAPI schema extensions
- Published as: NuGet package `Zen.CanonicaLib.UI`

**CanonicaLib.UI.Tests:**
- Purpose: Unit test coverage for generation logic
- Contains: Test classes using xUnit and Moq
- Key file: `DefaultSchemaGeneratorTests.cs` - Tests schema generation from types

**CanonicaLib.PackageComparer:**
- Purpose: Standalone CLI tool for analyzing package differences
- Contains: Services for assembly analysis, extraction, and report generation
- Executable: Published as .NET global tool

**Example/Contract:**
- Purpose: Sample canonical contract definitions
- Contains: Example controller interfaces, webhook definitions, sample models
- Used for: Documentation and testing the library

**Example/DesignSpecifications:**
- Purpose: OpenAPI design reference materials
- Contains: Design guidelines and specifications

**docs/:**
- Purpose: End-user documentation
- Contains: Getting started guides, tutorials
- Key file: `GETTINGSTARTED.md`

**.planning/codebase/:**
- Purpose: Internal GSD planning and analysis documents
- Contains: Architecture, structure, conventions, testing patterns, concerns, tech stack, integrations

## Key File Locations

**Entry Points:**

- `CanonicaLib.PackageComparer/Program.cs` - CLI application entry point
- No traditional `Startup.cs` or `Main()` in UI library (middleware-based integration)
- Web applications integrate via `app.MapCanonicaLib()` extension or manual handler registration

**Configuration:**

- `CanonicaLib.UI/CanonicaLibOptions.cs` - Central configuration class with page title, root path, API path, namespace filtering
- `Directory.Build.props` - Shared build properties across all projects (XML documentation, code analysis, etc.)

**Core Logic:**

- `CanonicaLib.UI/Services/DefaultDiscoveryService.cs` - Assembly and contract discovery (450+ lines)
- `CanonicaLib.UI/Services/DefaultDocumentGenerator.cs` - Orchestrates the generation pipeline
- `CanonicaLib.UI/Services/DefaultSchemaGenerator.cs` - Reflection-based schema generation (200+ lines)
- `CanonicaLib.UI/GeneratorContext.cs` - Shared state holder during generation

**Testing:**

- `CanonicaLib.UI.Tests/Services/DefaultSchemaGeneratorTests.cs` - Main test file
- `CanonicaLib.UI.Tests/TestModels/SelfReferencingEntity.cs` - Test fixture

**Data Annotations (Contracts):**

- `CanonicaLib.DataAnnotations/OpenApiPathAttribute.cs` - Mark interfaces as API controller definitions
- `CanonicaLib.DataAnnotations/OpenApiEndpointAttribute.cs` - Mark methods as endpoints with HTTP method
- `CanonicaLib.DataAnnotations/ILibrary.cs` - Required metadata provider interface
- `CanonicaLib.DataAnnotations/IService.cs` - Optional servers configuration
- `CanonicaLib.DataAnnotations/FromRequest*.cs` - Parameter binding attributes (4 variants)
- `CanonicaLib.DataAnnotations/Response*.cs` - Response metadata attributes (3 variants)

**HTTP Handlers:**

- `CanonicaLib.UI/Handlers/UIEndpointHandler.cs` - Renders main documentation page
- `CanonicaLib.UI/Handlers/AssembliesEndpointHandler.cs` - Returns JSON list of assemblies
- `CanonicaLib.UI/Handlers/AssemblyEndpointHandler.cs` - Returns OpenAPI spec for specific assembly
- `CanonicaLib.UI/Handlers/RedoclyEndpointHandler.cs` - Serves Redocly OpenAPI viewer
- `CanonicaLib.UI/Handlers/AttachmentEndpointHandler.cs` - Serves embedded resources

**Utilities:**

- `CanonicaLib.UI/Extensions/AssemblyExtensions.cs` - Reflection utilities
- `CanonicaLib.UI/Extensions/StringExtensions.cs` - String manipulation (slug conversion, empty checks)
- `CanonicaLib.UI/Extensions/ExampleAttributeExtensions.cs` - Example attribute processing

## Naming Conventions

**Files:**

- Attributes: `[Feature]Attribute.cs` (e.g., `OpenApiPathAttribute.cs`, `FromRequestBodyAttribute.cs`)
- Interfaces: `I[Feature].cs` (e.g., `IDiscoveryService.cs`, `ISchemaGenerator.cs`)
- Services: `Default[Feature]Service.cs` or `Default[Feature]Generator.cs` (e.g., `DefaultDiscoveryService.cs`, `DefaultSchemaGenerator.cs`)
- Models: `[Name]ViewModel.cs` or `[Name]Model.cs` (e.g., `AssembliesViewModel.cs`)
- Extensions: `[Feature]Extensions.cs` (e.g., `AssemblyExtensions.cs`)
- Views: `[PageName].cshtml` (e.g., `Index.cshtml`)
- Tests: `[ClassName]Tests.cs` (e.g., `DefaultSchemaGeneratorTests.cs`)

**Directories:**

- Feature domains: PascalCase (e.g., `Handlers/`, `Services/`, `Models/`)
- Sub-feature groups: PascalCase (e.g., `Interfaces/`, `Shared/`)
- Namespace hierarchy follows directory structure

**Namespaces:**

- Root: `Zen.CanonicaLib`
- Feature: `Zen.CanonicaLib.[Feature]` (e.g., `Zen.CanonicaLib.UI.Services`)
- Internal details: `Zen.CanonicaLib.[Feature].[SubFeature]` (e.g., `Zen.CanonicaLib.UI.Services.Interfaces`)

## Where to Add New Code

**New Generator Service:**
1. Create interface in `CanonicaLib.UI/Services/Interfaces/I[Feature]Generator.cs`
2. Create implementation in `CanonicaLib.UI/Services/Default[Feature]Generator.cs`
3. Register in DI container during middleware setup
4. Integrate into `DefaultDocumentGenerator` if it's part of main pipeline

**New Attribute/Annotation:**
1. Create class in `CanonicaLib.DataAnnotations/[Feature]Attribute.cs`
2. Inherit from appropriate base or implement target (method, class, etc.)
3. Add XML documentation with usage examples
4. Update discovery/generator logic in UI layer to handle new attribute

**New HTTP Handler:**
1. Create `CanonicaLib.UI/Handlers/[Feature]EndpointHandler.cs`
2. Implement static `async Task Handle[Feature]Request(HttpContext context)` method
3. Register in application middleware setup

**New View/Page:**
1. Create `.cshtml` file in `CanonicaLib.UI/Views/CanonicaLib/` (or `Shared/`)
2. Create corresponding ViewModel in `CanonicaLib.UI/Models/[Name]ViewModel.cs`
3. Register handler and routing to serve the view
4. Add navigation links in master layout if needed

**New Test:**
1. Create test class in `CanonicaLib.UI.Tests/Services/` or `TestModels/`
2. Use xUnit `[Fact]` or `[Theory]` attributes
3. Use Moq for service mocking
4. Use FluentAssertions for expressive assertions
5. Name test methods: `[MethodName]_[Condition]_[Expected]`

**Utilities/Extensions:**
1. Add methods to existing `CanonicaLib.UI/Extensions/[Domain]Extensions.cs` files
2. Use `public static` methods (extension methods)
3. Document with XML comments

**Example Contracts:**
1. Add interfaces to `Example/Contract/Controllers/` (for endpoints)
2. Add interfaces to `Example/Contract/Webhooks/` (for webhooks)
3. Add classes/models to `Example/Contract/Examples/`
4. Implement `ILibrary` in contract project for metadata

## Special Directories

**Services/Interfaces/:**
- Purpose: Contracts for all major services in the generation pipeline
- Generated: No (hand-written)
- Committed: Yes

**OpenApiExtensions/:**
- Purpose: Custom extensions to OpenAPI schema objects
- Generated: No
- Committed: Yes
- Allows extensibility of OpenAPI document format

**Views/:**
- Purpose: Razor templates for web UI rendering
- Generated: No (embedded in DLL)
- Committed: Yes
- Embedded as resources in `CanonicaLib.UI.csproj`

**bin/ and obj/:**
- Purpose: Build output directories
- Generated: Yes (build output)
- Committed: No (in .gitignore)

**Example/:**
- Purpose: Sample implementations and documentation
- Generated: No
- Committed: Yes
- Reference material for developers using the library

**.planning/codebase/:**
- Purpose: GSD analysis and planning documents
- Generated: Yes (created by mapping agent)
- Committed: Yes (valuable for future development)
- Read by `/gsd:plan-phase` and `/gsd:execute-phase` commands
