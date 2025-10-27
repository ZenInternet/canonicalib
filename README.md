# CanonicaLib

[![Build and Publish NuGet Packages](https://github.com/ZenInternet/canonicalib/actions/workflows/build-and-publish.yml/badge.svg)](https://github.com/ZenInternet/canonicalib/actions/workflows/build-and-publish.yml)
[![License](https://img.shields.io/badge/License-Apache_2.0-blue.svg)](https://opensource.org/licenses/Apache-2.0)
[![NuGet](https://img.shields.io/nuget/v/CanonicaLib.DataAnnotations.svg)](https://www.nuget.org/packages/CanonicaLib.DataAnnotations/)

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

All contributors must sign a Contributor License Agreement (CLA) that assigns copyright to Zen Internet Ltd to ensure we can maintain intellectual property ownership while keeping the project open source.

## 📄 License

This project is licensed under the Apache License 2.0 - see the [LICENSE](LICENSE) file for details.

**Copyright © 2025 Zen Internet Ltd**

Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at:

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.

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