# Contributing to CanonicaLib

Thank you for your interest in contributing to CanonicaLib! We welcome contributions from the community and are grateful for your support.

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Submitting Changes](#submitting-changes)
- [Style Guidelines](#style-guidelines)
- [Release Process](#release-process)
- [Getting Help](#getting-help)

## Code of Conduct

This project follows a Code of Conduct to ensure a welcoming and inclusive environment for all contributors. By participating, you agree to uphold this code.

## Getting Started

### Prerequisites

Before contributing, ensure you have:

- **.NET 8 SDK** or later installed
- **Git** for version control
- A suitable IDE (Visual Studio, VS Code, or JetBrains Rider)
- Basic understanding of C#, .NET Standard 2.1, and OpenAPI concepts

### Understanding the Project

CanonicaLib consists of two main packages:

1. **`CanonicaLib.DataAnnotations`** (.NET Standard 2.1)
   - Core attributes for defining canonical contracts
   - Platform-agnostic annotations
   - No external dependencies

2. **`CanonicaLib.UI`** (.NET 8)
   - OpenAPI generation and discovery services
   - Web UI for documentation and exploration
   - Runtime generation capabilities

## Development Setup

1. **Fork and Clone**
   ```bash
   git clone https://github.com/your-username/canonicalib.git
   cd canonicalib
   ```

2. **Build the Solution**
   ```bash
   dotnet build
   ```

3. **Run the Build**
   ```bash
   dotnet build
   ```

4. **Restore Packages**
   ```bash
   dotnet restore
   ```

### Project Structure

```
canonicalib/
├── CanonicaLib.DataAnnotations/    # Core attributes and interfaces
├── CanonicaLib.UI/                 # OpenAPI generation and web UI
├── Example/                        # Example implementation
├── docs/                          # Documentation
└── .github/                       # GitHub workflows and templates
```

## Making Changes

### Branching Strategy

- **`main`**: Stable release branch
- **`next`**: Development branch for upcoming features
- **Feature branches**: `feature/your-feature-name`
- **Bug fixes**: `fix/issue-description`
- **Documentation**: `docs/improvement-description`

### Workflow

1. **Create a Branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make Your Changes**
   - Keep changes focused and atomic
   - Write clear, self-documenting code
   - Add XML documentation for public APIs
   - Update related documentation

3. **Commit Your Changes**
   ```bash
   git add .
   git commit -m "feat: add new OpenAPI attribute for webhooks"
   ```

### Commit Message Format

We use [Conventional Commits](https://www.conventionalcommits.org/) for clear commit history:

```
type(scope): description

[optional body]

[optional footer]
```

**Types:**
- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting, etc.)
- `refactor`: Code refactoring
- `test`: Adding or updating tests
- `chore`: Maintenance tasks

**Examples:**
```
feat(dataannotations): add OpenApiWebhookAttribute for webhook documentation
fix(ui): resolve schema generation for nullable reference types
docs: update getting started guide with new examples
```

## Submitting Changes

### Pull Request Process

1. **Prepare Your Branch**
   ```bash
   git fetch origin
   git rebase origin/next
   git push origin feature/your-feature-name
   ```

2. **Create Pull Request**
   - Target the `next` branch (not `main`)
   - Use a clear, descriptive title
   - Fill out the PR template completely
   - Reference any related issues

3. **PR Requirements**
   - Code builds successfully
   - Documentation must be updated
   - At least one approving review required

### Pull Request Template

Your PR should include:

```markdown
## Description
Brief description of changes and motivation.

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
*Testing infrastructure will be added in future releases.*

## Documentation
- [ ] XML documentation updated
- [ ] README updated (if needed)
- [ ] CHANGELOG updated

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] No breaking changes (or documented)
```

## Style Guidelines

### C# Coding Standards

- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **nullable reference types** consistently
- Enable **treat warnings as errors**
- Maintain **clean architecture** principles

### Code Quality

```csharp
// ✅ Good - Clear, documented, follows conventions
/// <summary>
/// Marks a property as an OpenAPI parameter with metadata.
/// </summary>
/// <param name="name">The parameter name in the OpenAPI specification.</param>
/// <param name="description">Optional description of the parameter.</param>
[AttributeUsage(AttributeTargets.Property)]
public class OpenApiParameterAttribute : Attribute
{
    public string Name { get; }
    public string? Description { get; set; }

    public OpenApiParameterAttribute(string name)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }
}
```

### Documentation Standards

- **XML Documentation**: Required for all public APIs
- **README Updates**: For new features or breaking changes
- **Code Comments**: Explain complex logic, not obvious code
- **Examples**: Include usage examples in XML docs

## Release Process

### Versioning

We use [Semantic Versioning](https://semver.org/) with [MinVer](https://github.com/adamralph/minver):

- **Major** (X.0.0): Breaking changes
- **Minor** (X.Y.0): New features, backward compatible
- **Patch** (X.Y.Z): Bug fixes, backward compatible

### Release Workflow

1. **Development** happens on `next` branch
2. **Pull Requests** are merged to `next`
3. **Releases** are created by merging `next` to `main`
4. **Tags** are created automatically by GitHub Actions
5. **NuGet packages** are published automatically

### Breaking Changes

When making breaking changes:

1. **Document** the change thoroughly
2. **Update** major version number
3. **Provide** migration guide
4. **Announce** in release notes

## Getting Help

### Resources

- **Documentation**: See [Getting Started Guide](./docs/GETTINGSTARTED.md)
- **Examples**: Check the `Example/` directory
- **Issues**: Browse [GitHub Issues](https://github.com/ZenInternet/canonicalib/issues)
- **Discussions**: Join [GitHub Discussions](https://github.com/ZenInternet/canonicalib/discussions)

### Support Channels

- **Bug Reports**: [Create an issue](https://github.com/ZenInternet/canonicalib/issues/new?template=bug_report.md)
- **Feature Requests**: [Create an issue](https://github.com/ZenInternet/canonicalib/issues/new?template=feature_request.md)
- **Questions**: [Start a discussion](https://github.com/ZenInternet/canonicalib/discussions)

### Issue Templates

When creating issues, please:

- Use the appropriate template
- Provide detailed reproduction steps
- Include relevant code samples
- Specify environment details

## Thank You! 🎉

Your contributions help make CanonicaLib better for everyone. Whether you're fixing bugs, adding features, improving documentation, or helping other users, every contribution is valuable and appreciated.

---

**Copyright © 2025 Zen Internet Ltd**  
Licensed under the Apache License 2.0