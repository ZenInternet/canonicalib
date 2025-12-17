# Release Pipeline Documentation

## Overview

This repository uses an automated release pipeline that handles versioning and publishing based on branch strategy and pull request workflow.

## Branch Strategy

```
feature/* → next → main
   ↓         ↓      ↓
prerelease   rc   stable
```

### Branches

- **`feature/*`** - Feature development branches
- **`next`** - Integration/staging branch for testing features together
- **`main`** - Production-ready stable releases

## Versioning Strategy

The pipeline automatically determines version numbers based on the git workflow:

### Version Format

- **Stable**: `X.Y.0` (e.g., `1.2.0`)
- **RC (Release Candidate)**: `X.Y.0-rc.BUILD` (e.g., `1.2.0-rc.123`)
- **Prerelease**: `X.Y.0-prerelease.BUILD` (e.g., `1.2.0-prerelease.456`)

### Version Increments

Versions auto-increment the **MINOR** version (Y) by default:
- Latest tag: `v1.1.0`
- Next version: `1.2.0` (or `1.2.0-prerelease.*` / `1.2.0-rc.*`)

## Workflow Details

### 1. Feature Development (`feature/*` branches)

**When:** Developing new features

**Process:**
```bash
git checkout -b feature/my-awesome-feature
# ... make changes ...
git push origin feature/my-awesome-feature
```

**Result:**
- ✅ Build runs automatically
- ✅ Tests run
- ✅ Packages built with version `X.Y.0-prerelease.BUILD`
- ✅ Published to NuGet as prerelease
- 📦 Users can test with: `Install-Package CanonicaLib.UI -Version X.Y.0-prerelease.BUILD`

### 2. PR to `next` (Integration Testing)

**When:** Feature is ready for integration testing

**Process:**
```bash
# Create PR: feature/my-awesome-feature → next
```

**Result:**
- ✅ Build runs on PR
- ✅ Version: `X.Y.0-prerelease.BUILD`
- ✅ Packages published as prerelease
- 🧪 Team can test integrated changes

**After Merge:**
- Push to `next` triggers another prerelease build
- Integration tests can validate multiple features together

### 3. PR to `main` (Release Candidate)

**When:** `next` branch is stable and ready for release

**Process:**
```bash
# Create PR: next → main
```

**Result:**
- ✅ Build runs on PR
- ✅ Version: `X.Y.0-rc.BUILD`
- ✅ Packages published as Release Candidate
- 🎯 Final testing before production
- 📝 Review CHANGELOG and ensure version is correct

### 4. Merge to `main` (Stable Release)

**When:** RC testing is complete and approved

**Process:**
```bash
# Merge PR to main
```

**Result:**
- ✅ Build runs
- ✅ Version: `X.Y.0` (clean, no suffix)
- ✅ Git tag `vX.Y.0` created automatically
- ✅ GitHub Release created
- ✅ Packages published to NuGet as stable
- 📦 Production release available

## Examples

### Current state
- Latest tag: `v1.1.0`

### Scenario 1: Feature Development
```
feature/add-validation
  ↓ (push)
Build: 1.2.0-prerelease.789
Published: ✅ Prerelease

  ↓ (PR to next)
Build: 1.2.0-prerelease.790
Published: ✅ Prerelease

  ↓ (merge to next)
Build: 1.2.0-prerelease.791
Published: ✅ Prerelease
```

### Scenario 2: Release Process
```
next branch
  ↓ (PR to main)
Build: 1.2.0-rc.792
Published: ✅ RC

  ↓ (merge to main)
Build: 1.2.0
Tag: v1.2.0
Published: ✅ Stable Release
```

## Local Development

### Testing Version Generation Locally

```bash
# See what version would be generated
dotnet build -t:MinVer

# Build with specific version override (for testing)
dotnet build -p:MinVerVersion=1.99.0-test
```

### Manual Version Control

If you need to manually control versioning:

```bash
# Create a tag to set the base version
git tag v2.0.0
git push origin v2.0.0

# Future builds will be based on v2.0.0
# Next: 2.1.0-prerelease.*
# RC: 2.1.0-rc.*
# Release: 2.1.0
```

## CI/CD Pipeline Jobs

### `build` job
- Runs on: **all** pushes and PRs
- Calculates version based on branch/PR context
- Builds and tests the solution
- Packs NuGet packages
- Uploads packages as artifacts

### `publish-prerelease` job
- Runs on: feature/* branches, next branch, PRs to next
- Publishes packages with `-prerelease.BUILD` suffix
- Allows early testing of features

### `publish-rc` job
- Runs on: PRs to main
- Publishes packages with `-rc.BUILD` suffix
- Final validation before production release

### `release` job
- Runs on: merge to main
- Creates git tag `vX.Y.0`
- Creates GitHub Release
- Publishes stable packages (no suffix)

## Best Practices

### ✅ DO:
- Create feature branches for all new work
- Merge features to `next` for integration testing
- Test RC versions thoroughly before merging to `main`
- Update CHANGELOG.md before releasing to `main`
- Use semantic commit messages

### ❌ DON'T:
- Push directly to `main` (use PRs)
- Skip the RC phase for significant changes
- Manually create version tags (pipeline handles this)
- Delete version tags (breaks versioning history)

## Troubleshooting

### Version not incrementing?
- Check that latest tag exists: `git describe --tags --abbrev=0`
- Ensure you've fetched tags: `git fetch --tags`

### Build failing?
- Check GitHub Actions logs
- Verify tests pass locally: `dotnet test`
- Ensure NuGet API key is configured in repository secrets

### Need to rollback a release?
```bash
# Unlist the package on NuGet.org (don't delete)
# Create a hotfix branch and new release
git checkout -b hotfix/fix-issue main
# ... fix ...
# Follow normal PR process to main
```

## Version History

See [CHANGELOG.md](../CHANGELOG.md) for detailed version history.
