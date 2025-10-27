# CanonicaLib

**CanonicaLib** is an open-source library that enables systems development teams to create and maintain canonical shared libraries across multiple teams and projects. It provides tooling to transform C# source code containing model definitions, API endpoints, and webhook specifications into standards-compliant packages for different platforms.

## 🎯 Purpose

CanonicaLib addresses the challenge of maintaining consistent data models, API contracts, and shared capabilities across multiple development teams. By defining contracts once in C#, teams can automatically generate:

- **Standards-compliant .NET NuGet packages**
- **OpenAPI specifications**
- **TypeScript NPM packages** *(planned)*

## ✨ Features

- **Canonical Model Definitions**: Define your data models once using C# classes with custom attributes
- **API Contract Generation**: Automatically generate OpenAPI specifications from attributed interface definitions
- **Cross-Platform Package Generation**: Create packages for multiple platforms from a single source
- **Discovery Service**: Automatically discover and process canonical assemblies in your application domain
- **Schema Generation**: Generate JSON schemas and OpenAPI components from your C# models

## 🏗️ Architecture

The library consists of several key components:

### Core Packages

- **`CanonicaLib.DataAnnotations`** (.NET Standard 2.1): Contains the core attributes for marking canonical contracts
- **`CanonicaLib.UI`** (.NET 8): Provides services for generating OpenAPI specifications and discovering canonical definitions

### Key Services

- **`DiscoveryService`**: Finds and identifies canonical assemblies and their definitions
- **`SchemaGenerator`**: Converts C# types to OpenAPI schema definitions
- **`SchemasGenerator`**: Orchestrates schema generation for entire assemblies
- **`ComponentsGenerator`**: Creates OpenAPI components from discovered schemas

## 🚀 Getting Started

Please see the [Getting Started Guide]()./docs/GETTINGSTARTED.md) to get started

## 🤝 Contributing

We welcome contributions! Please see our [Contributing Guidelines](./docs/CONTRIBUTING.md) for details on:

- Code style and standards
- Submitting pull requests
- Reporting issues
- Development setup

## 📋 Roadmap

- [ ] TypeScript NPM package generation
- [ ] Enhanced webhook support
- [ ] Additional schema validation
- [ ] CLI tooling for package generation
- [ ] Integration with popular CI/CD pipelines

## 🏷️ Version History

See [CHANGELOG.md](CHANGELOG.md) for a detailed history of changes and releases.

---

**CanonicaLib** - Building canonical contracts for modern development teams.