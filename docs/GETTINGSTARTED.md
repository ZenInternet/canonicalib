# Getting Started Guide

## Prerequisites

- .NET Standard 2.1 or later
- .NET 8 (for UI components)

## Suggested Solution

The recommended approach consists of a single .NET solution file containing your library packages, which can be sliced to suit your delivery function - whether domain-based or product-based.

### Solution Structure

```
YourSolution.sln
├── MyLibrary/                 # Globally shared contracts
├── MyLibrary.Domain1/         # Domain-specific contracts
├── MyLibrary.Domain2/         # Additional domain contracts
├── MyLibrary.Products/        # Product-specific contracts
└── MyCanonicaLib/             # Minimal web project for CanonicaLib UI
```

The web host project serves as your canonical contract documentation hub, automatically discovering and presenting all contracts defined across your library projects.

The model projects (MyLibrary, MyLibrary.Domain1, etc.) are designed to be packed as NuGet packages for consumption by .NET APIs and clients throughout your ecosystem.

### Key Requirements

- **Single Solution**: Keep all your canonical libraries within one solution for easy management
- **Flexible Organisation**: Structure projects by domain, product, or any organisational pattern that suits your team, include interdependencies as project references
- **Minimal Web Host**: Include a single minimal web project to host the CanonicaLib UI for documentation and exploration

## Web Project Setup

In your `MyCanonicaLib` project:

Install the UI package for OpenAPI generation

```shell
dotnet add package CanonicaLib.UI
```

Add CanonicaLib to the Program.cs
```csharp
using Zen.CanonicaLib.UI;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCanonicaLib(() =>
    new Zen.CanonicaLib.UI.WebApplicationOptions()
    {
        PageTitle = "Zen's Canonical Libraries"
    }
);

var app = builder.Build();
app.UseCanonicaLib();
app.Run();
```

## Models Projects Setup

Install the core annotations package
```shell
dotnet add package CanonicaLib.DataAnnotations
```

Ensure that the `.csproj` file contains the following:
```xml
<ItemGroup>
  <EmbeddedResource Include="Docs\**\*.md" />
</ItemGroup>
```