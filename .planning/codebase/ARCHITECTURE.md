# Architecture

**Analysis Date:** 2026-02-17

## Pattern Overview

**Overall:** Reflection-based OpenAPI Generation with Attribute-Driven Contracts

CanonicaLib follows a **layered architecture** that uses C# attributes and reflection to transform source code definitions into cross-platform packages. The system is built on the principle that developers define canonical contracts once using C# interfaces and classes, and the library generates OpenAPI documentation, schemas, and potentially other platform packages from these definitions.

**Key Characteristics:**
- **Attribute-Driven Contract Definition:** Contracts are defined using custom attributes applied to interfaces and types
- **Reflection-Based Code Analysis:** Runtime reflection discovers types marked with canonical attributes
- **Service-Oriented Generator Pipeline:** Specialized services handle different aspects of OpenAPI document generation
- **Pluggable Processing Chain:** Post-processors allow customization of generated OpenAPI elements
- **Assembly Discovery and Introspection:** Automatic discovery of canonical assemblies loaded in the application domain

## Layers

**Annotation Layer (CanonicaLib.DataAnnotations):**
- Purpose: Defines the contract language for developers
- Location: `CanonicaLib.DataAnnotations/`
- Contains: Attribute classes, core interfaces (ILibrary, IService, ISecureService, IExample)
- Depends on: Microsoft.OpenApi (for OpenApiServer, OpenApiLicense, OpenApiTag references)
- Used by: All projects that want to define canonical contracts
- Key Files:
  - `OpenApiPathAttribute.cs` - Marks interfaces as API controller definitions
  - `OpenApiEndpointAttribute.cs` - Marks methods as API endpoints with HTTP method
  - `FromRequestBodyAttribute.cs`, `FromRequestQueryAttribute.cs`, `FromRequestPathAttribute.cs`, `FromRequestHeaderAttribute.cs` - Parameter binding
  - `ResponseAttribute.cs`, `ResponseExampleAttribute.cs`, `ResponseHeaderAttribute.cs` - Response metadata
  - `ILibrary.cs` - Provides metadata for library documentation
  - `IService.cs` - Provides OpenAPI servers configuration
  - `ISecureService.cs` - Security configuration
  - `IExample.cs` - Example generation capability

**Generation Layer (CanonicaLib.UI):**
- Purpose: Analyzes assemblies and generates OpenAPI documents
- Location: `CanonicaLib.UI/`
- Contains: Discovery service, generator services, request handlers, models
- Depends on: CanonicaLib.DataAnnotations, Microsoft.OpenApi, Namotion.Reflection, ASP.NET Core
- Used by: Web applications that expose CanonicaLib UI
- Key Components:
  - **Discovery Service** (`Services/DefaultDiscoveryService.cs`) - Assembly and contract introspection
  - **Generator Services** (`Services/Default*Generator.cs`) - Orchestrate OpenAPI document creation
  - **Handlers** (`Handlers/*EndpointHandler.cs`) - HTTP request routing for UI and API endpoints
  - **Models** (`Models/*ViewModel.cs`) - View models for Razor rendering
  - **Extensions** (`Extensions/*`) - Utility methods for reflection, string manipulation, examples

**Presentation Layer (CanonicaLib.UI Views):**
- Purpose: User interface for browsing generated OpenAPI documentation
- Location: `CanonicaLib.UI/Views/`
- Contains: Razor views (.cshtml files)
- Depends on: ASP.NET Core Razor, Bootstrap/CSS frameworks
- Used by: Browser clients accessing the documentation UI
- Key Views:
  - `CanonicaLib/Index.cshtml` - Main landing page listing all canonical assemblies
  - `CanonicaLib/Redocly.cshtml` - OpenAPI documentation viewer using Redocly
  - `Shared/_Layout.cshtml` - Master layout template

**Tool Layer (CanonicaLib.PackageComparer):**
- Purpose: Utility for comparing and analyzing package versions
- Location: `CanonicaLib.PackageComparer/`
- Contains: CLI tool for package comparison analysis
- Depends on: .NET Standard 2.1+
- Key Files:
  - `Program.cs` - Entry point
  - `Services/AssemblyAnalyzer.cs` - Assembly introspection
  - `Services/PackageExtractor.cs` - Extract assembly from packages
  - `Services/ComparisonReporter.cs` - Generate comparison reports

**Example/Reference Layer:**
- Purpose: Demonstrate how to use CanonicaLib
- Location: `Example/`
- Contains: Sample contract definitions and design specifications
- Key Projects:
  - `Example/Contract/` - Example canonical contract definitions
  - `Example/DesignSpecifications/` - OpenAPI design specifications

## Data Flow

**Assembly Discovery and OpenAPI Generation:**

1. **Discovery Phase**
   - `IDiscoveryService.GetAllAssemblies()` loads all DLLs from app domain
   - `FindCanonicalAssemblies()` filters to assemblies referencing `Zen.CanonicaLib.DataAnnotations`
   - `FindControllerDefinitions()` finds interfaces marked with `@OpenApiPathAttribute`
   - `FindSchemaDefinitions()` finds classes/enums/structs to document as schemas
   - `GetLibraryInstance()` instantiates `ILibrary` implementation for metadata

2. **Document Generation Phase**
   - `IDocumentGenerator.GenerateDocument(Assembly)` creates `GeneratorContext`
   - `IInfoGenerator` adds OpenAPI info (title, version, description)
   - `ISchemasGenerator` calls `ISchemaGenerator` for each schema type
   - `ISchemaGenerator.GenerateSchema()` uses reflection to create OpenAPI schema objects
   - `IPathsGenerator` processes controller interfaces and their endpoints
   - `IOperationGenerator` creates operation definitions for each endpoint method
   - `IParametersGenerator`, `IRequestBodyGenerator`, `IResponsesGenerator` handle HTTP details
   - `ISecurityGenerator` adds security schemes from `ISecureService`
   - `IServersGenerator` adds server URLs from `IService`
   - `IWebhooksGenerator` processes webhook definitions marked with `@OpenApiWebhookAttribute`

3. **Schema Processing**
   - Reflection analyzes type structure (properties, fields, methods)
   - Primitive types mapped to OpenAPI primitive schemas
   - Classes/structs analyzed for properties (recursively)
   - Enums converted to enumeration schemas
   - Existing schemas reused via reference (avoid duplication)
   - XML documentation extracted for descriptions
   - Examples generated from `IExample<T>` implementations

4. **HTTP Request Handling**
   - Router maps incoming requests to endpoint handlers
   - `AssembliesEndpointHandler` returns JSON list of canonical assemblies
   - `AssemblyEndpointHandler` returns OpenAPI spec for specific assembly
   - `UIEndpointHandler` renders Razor view with assembly listing
   - `RedoclyEndpointHandler` serves interactive OpenAPI UI
   - `AttachmentEndpointHandler` serves embedded resources (images, docs)

**State Management:**

State is managed through:
- **GeneratorContext** - Passed through the entire generation pipeline
  - Holds reference to the target assembly being processed
  - Accumulates generated OpenAPI document
  - Maintains schema registry to prevent duplicate schema generation
- **IDiscoveryService** - Stateless discovery of contracts
- **Options Object (CanonicaLibOptions)** - Configuration for UI paths, namespaces, post-processors
- **Dependency Injection** - ASP.NET Core DI container manages service lifetime

## Key Abstractions

**IDiscoveryService:**
- Purpose: Abstracts assembly and contract introspection
- Examples: `DefaultDiscoveryService` (only implementation)
- Pattern: Strategy pattern for discovering contracts across loaded assemblies
- Methods: `FindCanonicalAssemblies()`, `FindControllerDefinitions()`, `FindSchemaDefinitions()`, `GetLibraryInstance()`, etc.

**IDocumentGenerator:**
- Purpose: Orchestrates the complete OpenAPI document generation
- Examples: `DefaultDocumentGenerator` (only implementation)
- Pattern: Orchestrator pattern composing multiple specialized generators
- Responsibility: Coordinates info, schemas, paths, security, webhooks, servers, and tag groups generators

**Generator Services (ISchemaGenerator, IPathsGenerator, IOperationGenerator, etc.):**
- Purpose: Each handles one aspect of OpenAPI document creation
- Examples: `Default*Generator` classes in `Services/`
- Pattern: Chain of Responsibility / Pipeline
- Each generator focuses on a specific OpenAPI section and delegates to sub-generators

**CanonicaLibOptions:**
- Purpose: Configuration abstraction for UI behavior
- Pattern: Options/Configuration pattern
- Properties: PageTitle, RootPath, ApiPath, RootNamespace, PostProcessors
- PostProcessors allow injection of custom processing logic

**ILibrary:**
- Purpose: Metadata provider contract for documented libraries
- Pattern: Provider pattern for customization
- Required: One per canonical assembly
- Provides: FriendlyName, TagGroups, License information

**IService / ISecureService:**
- Purpose: Server and security configuration providers
- Pattern: Provider pattern
- Optional: Zero or one per canonical assembly
- Provides: OpenAPI servers, security schemes

**IExample<T>:**
- Purpose: Example data provider for test data
- Pattern: Strategy pattern for generating examples
- Used by: ExamplesGenerator to populate example values in documentation

## Entry Points

**Web Application Integration:**
- Location: Typically in `Program.cs` or middleware registration
- Triggers: HTTP requests to configured paths
- Responsibilities:
  - Register CanonicaLib services in DI container
  - Add CanonicaLib middleware/endpoints to request pipeline
  - Serve UI at configured root path (default `/canonicalib`)
  - Serve API specifications at configured API path (default `/canonicalib/api`)

**Assembly Entry Points:**
- **CanonicaLib.DataAnnotations** - No entry point, pure contract definitions
- **CanonicaLib.UI** - Middleware registration: `app.MapCanonicaLib()` or manual handler registration
- **CanonicaLib.PackageComparer** - `Program.cs` - CLI tool entry point with command-line argument parsing

**Request Handler Entry Points:**
- `UIEndpointHandler.HandleUIRequest()` - `/canonicalib` (or configured root)
- `AssembliesEndpointHandler.HandleAssembliesRequest()` - `/canonicalib/api/assemblies`
- `AssemblyEndpointHandler.HandleAssemblyRequest()` - `/canonicalib/api/assemblies/{name}`
- `RedoclyEndpointHandler.HandleRedoclyRequest()` - `/canonicalib/{name}`

## Error Handling

**Strategy:** Explicit exception throwing with descriptive messages, combined with logging

**Patterns:**

1. **Discovery Service Validation:**
   - `GetLibraryInstance()` throws `InvalidOperationException` if zero or multiple implementations found
   - `GetServiceInstance()` returns null if not found (optional), throws if multiple found
   - `FindAttachmentInAssembly()` throws `FileNotFoundException` if resource not found

2. **Generation Service Error Handling:**
   - `GenerateSchema()` wraps reflection errors in `InvalidOperationException` with context
   - Parameter and type validation with `ArgumentNullException` for null inputs
   - Logging at Information and Warning levels for diagnostics

3. **HTTP Handler Error Handling:**
   - View resolution failures return 500 status with searched locations listed
   - Resource loading failures logged and surfaces as HTTP errors
   - JSON serialization errors propagated as HTTP 500

4. **Validation:**
   - `CanonicaLibOptions` uses `System.ComponentModel.DataAnnotations` attributes
   - RootPath validated with regex: must start with `/`, not end with `/` (except `/`)
   - ApiPath validated with regex: must start with `/`, must not end with `/`
   - StringLength constraints on PageTitle (1-200) and RootNamespace (max 200)

## Cross-Cutting Concerns

**Logging:**
- Framework: Microsoft.Extensions.Logging
- Usage: Information level for generation steps, errors logged with full context
- Services: `DefaultSchemaGenerator`, `DefaultDocumentGenerator` use injected `ILogger<T>`

**Validation:**
- Types: Data annotations on `CanonicaLibOptions` for configuration validation
- Contracts: Attribute presence validation in discovery service
- Reflection: Type checks for interfaces, classes, enums in schema generation

**Reflection:**
- Heavy use throughout for dynamic type analysis
- Namotion.Reflection library for XML documentation extraction
- Type.GetCustomAttribute<T>() for attribute introspection
- AppDomain.CurrentDomain.BaseDirectory for assembly discovery

**Extensibility:**
- Post-processors pattern in `CanonicaLibOptions.PostProcessors`
- Interface-based design allows alternate implementations of all major components
- DI container integration allows customization at registration time
