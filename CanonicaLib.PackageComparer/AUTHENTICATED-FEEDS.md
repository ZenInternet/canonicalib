# Using Authenticated NuGet Feeds

The PackageComparer tool supports authenticated NuGet feeds, including private Azure DevOps feeds and other authenticated sources.

## Azure DevOps Pipelines with NuGetAuthenticate

When running in an Azure DevOps pipeline, use the `NuGetAuthenticate@1` task before running the PackageComparer:

```yaml
steps:
- task: NuGetAuthenticate@1
  displayName: 'Authenticate with NuGet feeds'

- script: |
    canonicalib-comparer MyPackage/1.0.0 MyPackage/2.0.0
  displayName: 'Compare packages'
```

The `NuGetAuthenticate` task automatically configures credentials for all feeds defined in your `nuget.config` file.

## Using nuget.config

### Option 1: Project-level nuget.config

Create a `nuget.config` file in your project directory:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <clear />
    <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
    <add key="MyPrivateFeed" value="https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json" />
  </packageSources>
</configuration>
```

### Option 2: User-level nuget.config

Configure feeds at the user level:

**Windows:**
```
%APPDATA%\NuGet\NuGet.Config
```

**Linux/macOS:**
```
~/.nuget/NuGet/NuGet.Config
```

## Authentication Methods

### 1. Azure DevOps Personal Access Token (PAT)

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <packageSources>
    <add key="MyAzureFeed" value="https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json" />
  </packageSources>
  <packageSourceCredentials>
    <MyAzureFeed>
      <add key="Username" value="anystring" />
      <add key="ClearTextPassword" value="YOUR_PAT_HERE" />
    </MyAzureFeed>
  </packageSourceCredentials>
</configuration>
```

**Security Note:** Store PATs in environment variables instead:

```xml
<MyAzureFeed>
  <add key="Username" value="%NUGET_USERNAME%" />
  <add key="ClearTextPassword" value="%NUGET_PASSWORD%" />
</MyAzureFeed>
```

### 2. Windows Integrated Authentication

For on-premises NuGet servers:

```xml
<packageSourceCredentials>
  <MyFeed>
    <add key="Username" value="DOMAIN\username" />
    <add key="Password" value="encryptedPassword" />
  </MyFeed>
</packageSourceCredentials>
```

### 3. API Key Authentication

Some private feeds use API keys:

```xml
<apikeys>
  <add key="https://my-private-feed.com/v3/index.json" value="YOUR_API_KEY" />
</apikeys>
```

## Using Azure Artifacts Credential Provider

For Azure DevOps feeds, install the Azure Artifacts Credential Provider:

### Windows

```powershell
iex "& { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }"
```

### Linux/macOS

```bash
wget -qO- https://aka.ms/install-artifacts-credprovider.sh | bash
```

Once installed, the credential provider automatically handles authentication for Azure Artifacts feeds.

## Command-Line Usage

### Use All Configured Feeds

By default, the tool uses all enabled feeds from your `nuget.config`:

```bash
canonicalib-comparer MyPackage/1.0.0 MyPackage/2.0.0
```

The tool will try each configured feed until it finds the package.

### Specify a Single Feed

Override configured feeds with the `-s` or `--source` option:

```bash
canonicalib-comparer MyPackage/1.0.0 MyPackage/2.0.0 \
  --source https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json
```

**Note:** When using `--source`, credentials from `nuget.config` are still used if configured for that URL.

## DevOps Pipeline Examples

### Azure DevOps

```yaml
trigger:
  - main

pool:
  vmImage: 'ubuntu-latest'

steps:
- task: NuGetAuthenticate@1

- task: DotNetCoreCLI@2
  displayName: 'Install PackageComparer'
  inputs:
    command: 'custom'
    custom: 'tool'
    arguments: 'install -g Zen.CanonicaLib.PackageComparer'

- script: |
    canonicalib-comparer \
      Zen.CanonicaLib.UI/1.2.0 \
      Zen.CanonicaLib.UI/1.3.0 \
      -f markdown \
      -o comparison-report.md
  displayName: 'Compare package versions'

- task: PublishBuildArtifacts@1
  inputs:
    PathtoPublish: 'comparison-report.md'
    ArtifactName: 'comparison-report'
```

### GitHub Actions with Azure Artifacts

```yaml
name: Compare Packages

on: [push]

jobs:
  compare:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
          source-url: https://pkgs.dev.azure.com/myorg/_packaging/myfeed/nuget/v3/index.json
        env:
          NUGET_AUTH_TOKEN: ${{ secrets.AZURE_DEVOPS_PAT }}
      
      - name: Install PackageComparer
        run: dotnet tool install -g Zen.CanonicaLib.PackageComparer
      
      - name: Compare packages
        run: |
          canonicalib-comparer \
            MyPackage/1.0.0 \
            MyPackage/2.0.0 \
            -f json \
            -o report.json
```

## Troubleshooting

### "Failed to download package from any configured source"

1. **Check feed URL:** Ensure the feed URL is correct in `nuget.config`
2. **Verify credentials:** Test authentication with `dotnet restore` first
3. **Check permissions:** Ensure your account/token has read access to the feed
4. **Enable verbose output:** Use `-v` flag to see detailed error messages

```bash
canonicalib-comparer Package/1.0 Package/2.0 -v
```

### "401 Unauthorized"

- PAT may be expired - regenerate in Azure DevOps
- Credentials may be missing from `nuget.config`
- Credential provider may not be installed

### "403 Forbidden"

- Account doesn't have permission to access the feed
- Feed may be scoped to a specific project/organization

### Testing Feed Access

Test your feed configuration with dotnet CLI:

```bash
# List packages from a specific source
dotnet nuget list source

# Try restoring a package
dotnet add package YourPackage --source https://your-feed-url
```

## Security Best Practices

1. **Never commit credentials to source control**
2. **Use environment variables** for PATs and passwords
3. **Use credential providers** when available (Azure Artifacts)
4. **Rotate PATs regularly**
5. **Use pipeline-scoped tokens** in CI/CD environments
6. **Encrypt sensitive values** in `nuget.config` using `nuget.exe sources update -ConfigFile`

## Additional Resources

- [NuGet Configuration Files](https://docs.microsoft.com/en-us/nuget/reference/nuget-config-file)
- [Azure Artifacts Credential Provider](https://github.com/microsoft/artifacts-credprovider)
- [Azure DevOps PAT Documentation](https://docs.microsoft.com/en-us/azure/devops/organizations/accounts/use-personal-access-tokens-to-authenticate)
