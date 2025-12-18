using NuGet.Common;
using NuGet.Configuration;
using NuGet.Credentials;
using NuGet.Packaging;
using NuGet.Packaging.Core;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Zen.CanonicaLib.PackageComparer.Models;

namespace Zen.CanonicaLib.PackageComparer.Services;

public class PackageExtractor
{
    private readonly IEnumerable<PackageSource> _packageSources;
    private readonly ISettings _settings;
    private static bool _credentialProvidersInitialized = false;

    public PackageExtractor(string? packageSource = null)
    {
        // Initialize credential providers once (for Azure DevOps authentication)
        if (!_credentialProvidersInitialized)
        {
            DefaultCredentialServiceUtility.SetupDefaultCredentialService(NullLogger.Instance, nonInteractive: true);
            _credentialProvidersInitialized = true;
        }
        
        // Load NuGet settings from default locations (nuget.config files)
        _settings = Settings.LoadDefaultSettings(root: null);
        
        if (packageSource != null)
        {
            // Use the explicitly provided package source
            _packageSources = new[] { new PackageSource(packageSource) };
        }
        else
        {
            // Use all configured package sources from NuGet.config
            // This includes sources with credentials from NuGetAuthenticate
            var packageSourceProvider = new PackageSourceProvider(_settings);
            var sources = packageSourceProvider.LoadPackageSources().Where(s => s.IsEnabled).ToList();
            
            if (!sources.Any())
            {
                // Fallback to nuget.org if no sources are configured
                _packageSources = new[] { new PackageSource("https://api.nuget.org/v3/index.json") };
            }
            else
            {
                _packageSources = sources;
            }
        }
    }

    public async Task<PackageInfo> ExtractPackageAsync(string packageIdentifier)
    {
        // Check if it's a DLL file path
        if (File.Exists(packageIdentifier) && packageIdentifier.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
        {
            return ExtractSingleAssembly(packageIdentifier);
        }

        // Check if it's a .nupkg file path
        if (File.Exists(packageIdentifier) && packageIdentifier.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
        {
            return await ExtractLocalPackageAsync(packageIdentifier);
        }

        // Parse package ID and version from format: PackageId/Version or PackageId/latest
        var parts = packageIdentifier.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Package identifier must be in format 'PackageId/Version' (or 'PackageId/latest'), a path to .nupkg file, or a path to .dll file");
        }

        var packageId = parts[0];
        var versionString = parts[1];

        // If version is "latest", resolve to the latest stable version
        if (versionString.Equals("latest", StringComparison.OrdinalIgnoreCase))
        {
            versionString = await GetLatestVersionAsync(packageId);
            Console.WriteLine($"Resolved 'latest' to version: {versionString}");
        }

        return await DownloadAndExtractPackageAsync(packageId, versionString);
    }

    private async Task<string> GetLatestVersionAsync(string packageId)
    {
        var cache = new SourceCacheContext();
        Exception? lastException = null;

        foreach (var source in _packageSources)
        {
            try
            {
                // Create repository with proper authentication
                var packageSourceProvider = new PackageSourceProvider(_settings);
                var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
                var repository = sourceRepositoryProvider.CreateRepository(source);
                
                var metadataResource = await repository.GetResourceAsync<PackageMetadataResource>();

                var packages = await metadataResource.GetMetadataAsync(
                    packageId,
                    includePrerelease: false,
                    includeUnlisted: false,
                    cache,
                    NullLogger.Instance,
                    CancellationToken.None);

                var latestPackage = packages
                    .OrderByDescending(p => p.Identity.Version)
                    .FirstOrDefault();

                if (latestPackage != null)
                {
                    return latestPackage.Identity.Version.ToString();
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine($"Failed to get metadata from {source.Source}: {ex.Message}");
            }
        }

        throw new Exception($"Failed to find latest version for package '{packageId}' from any configured source. Last error: {lastException?.Message}");
    }

    private PackageInfo ExtractSingleAssembly(string dllPath)
    {
        var fileName = Path.GetFileNameWithoutExtension(dllPath);
        var extractPath = Path.Combine(Path.GetTempPath(), $"dll_compare_{Guid.NewGuid()}");
        Directory.CreateDirectory(extractPath);

        // Copy the DLL to the temp location
        var targetPath = Path.Combine(extractPath, Path.GetFileName(dllPath));
        File.Copy(dllPath, targetPath);

        return new PackageInfo
        {
            PackageId = fileName,
            Version = "Direct DLL",
            ExtractPath = extractPath,
            AssemblyPaths = new List<string> { targetPath }
        };
    }

    private async Task<PackageInfo> ExtractLocalPackageAsync(string nupkgPath)
    {
        var extractPath = Path.Combine(Path.GetTempPath(), $"pkg_compare_{Guid.NewGuid()}");
        Directory.CreateDirectory(extractPath);

        using var packageStream = File.OpenRead(nupkgPath);
        using var packageReader = new PackageArchiveReader(packageStream);

        var identity = await packageReader.GetIdentityAsync(CancellationToken.None);
        
        // Extract all files
        var files = await packageReader.GetFilesAsync(CancellationToken.None);
        foreach (var file in files)
        {
            var targetPath = Path.Combine(extractPath, file);
            var targetDir = Path.GetDirectoryName(targetPath);
            if (targetDir != null && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            using var sourceStream = packageReader.GetStream(file);
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);
        }

        // Download dependencies
        await DownloadDependenciesAsync(packageReader, extractPath);

        return new PackageInfo
        {
            PackageId = identity.Id,
            Version = identity.Version.ToString(),
            ExtractPath = extractPath,
            AssemblyPaths = GetAssemblyPaths(extractPath)
        };
    }

    private async Task<PackageInfo> DownloadAndExtractPackageAsync(string packageId, string versionString)
    {
        var cache = new SourceCacheContext();
        
        if (!NuGetVersion.TryParse(versionString, out var nugetVersion))
        {
            throw new ArgumentException($"Invalid version format: {versionString}");
        }

        var extractPath = Path.Combine(Path.GetTempPath(), $"pkg_compare_{Guid.NewGuid()}");
        Directory.CreateDirectory(extractPath);

        var packagePath = Path.Combine(extractPath, $"{packageId}.{versionString}.nupkg");

        // Try each package source until we find the package
        Exception? lastException = null;
        bool downloaded = false;

        foreach (var source in _packageSources)
        {
            try
            {
                // Create repository with proper authentication
                var packageSourceProvider = new PackageSourceProvider(_settings);
                var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
                var repository = sourceRepositoryProvider.CreateRepository(source);
                
                var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

                using (var packageStream = File.Create(packagePath))
                {
                    var success = await resource.CopyNupkgToStreamAsync(
                        packageId,
                        nugetVersion,
                        packageStream,
                        cache,
                        NullLogger.Instance,
                        CancellationToken.None);

                    if (success)
                    {
                        downloaded = true;
                        Console.WriteLine($"Downloaded from: {source.Source}");
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                lastException = ex;
                Console.WriteLine($"Failed to download from {source.Source}: {ex.Message}");
                // Try next source
            }
        }

        if (!downloaded)
        {
            throw new Exception($"Failed to download package {packageId}/{versionString} from any configured source. Last error: {lastException?.Message}");
        }

        using var fileStream = File.OpenRead(packagePath);
        using var packageReader = new PackageArchiveReader(fileStream);

        var files = await packageReader.GetFilesAsync(CancellationToken.None);
        foreach (var file in files)
        {
            var targetPath = Path.Combine(extractPath, file);
            var targetDir = Path.GetDirectoryName(targetPath);
            if (targetDir != null && !Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            using var sourceStream = packageReader.GetStream(file);
            using var targetStream = File.Create(targetPath);
            await sourceStream.CopyToAsync(targetStream);
        }

        // Download dependencies
        await DownloadDependenciesAsync(packageReader, extractPath);

        return new PackageInfo
        {
            PackageId = packageId,
            Version = versionString,
            ExtractPath = extractPath,
            AssemblyPaths = GetAssemblyPaths(extractPath)
        };
    }

    private async Task DownloadDependenciesAsync(PackageArchiveReader packageReader, string extractPath)
    {
        try
        {
            var dependenciesFolder = Path.Combine(extractPath, "dependencies");
            Directory.CreateDirectory(dependenciesFolder);

            var cache = new SourceCacheContext();
            var nuspecReader = await packageReader.GetNuspecReaderAsync(CancellationToken.None);
            var dependencyGroups = nuspecReader.GetDependencyGroups();

            var downloadedPackages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var failedDependencies = new List<string>();

            // Pick the most compatible dependency group (prefer netstandard2.1, then netstandard2.0, then first available)
            var targetGroup = dependencyGroups
                .OrderByDescending(g => g.TargetFramework.Framework == ".NETStandard" && g.TargetFramework.Version.Major == 2 && g.TargetFramework.Version.Minor == 1)
                .ThenByDescending(g => g.TargetFramework.Framework == ".NETStandard" && g.TargetFramework.Version.Major == 2 && g.TargetFramework.Version.Minor == 0)
                .FirstOrDefault();

            if (targetGroup != null)
            {
                Console.WriteLine($"Processing dependency group for {targetGroup.TargetFramework}...");
                await DownloadDependencyGroupAsync(targetGroup, dependenciesFolder, extractPath, cache, downloadedPackages, failedDependencies, depth: 0);
            }

            if (failedDependencies.Any())
            {
                Console.WriteLine($"\nWarning: {failedDependencies.Count} dependencies could not be downloaded:");
                foreach (var dep in failedDependencies)
                {
                    Console.WriteLine($"  - {dep}");
                }
                Console.WriteLine("Assembly analysis may be incomplete.\n");
            }
        }
        catch (Exception ex)
        {
            // Don't fail the entire operation if dependency download fails
            Console.WriteLine($"Warning: Error during dependency resolution: {ex.Message}");
        }
    }

    private async Task DownloadDependencyGroupAsync(
        PackageDependencyGroup dependencyGroup,
        string dependenciesFolder,
        string extractPath,
        SourceCacheContext cache,
        HashSet<string> downloadedPackages,
        List<string> failedDependencies,
        int depth)
    {
        if (depth > 5) // Prevent infinite recursion
            return;

        var indent = new string(' ', depth * 2);
        
        foreach (var dependency in dependencyGroup.Packages)
        {
            var dependencyKey = $"{dependency.Id}/{dependency.VersionRange.MinVersion}";
            if (downloadedPackages.Contains(dependencyKey))
                continue;

            try
            {
                // Try to download the dependency
                var versionToDownload = dependency.VersionRange.MinVersion;
                if (versionToDownload == null)
                    continue;

                bool downloaded = false;
                Exception? lastError = null;

                foreach (var source in _packageSources)
                {
                    try
                    {
                        var packageSourceProvider = new PackageSourceProvider(_settings);
                        var sourceRepositoryProvider = new SourceRepositoryProvider(packageSourceProvider, Repository.Provider.GetCoreV3());
                        var repository = sourceRepositoryProvider.CreateRepository(source);
                        var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

                        var dependencyPackagePath = Path.Combine(dependenciesFolder, $"{dependency.Id}.{versionToDownload}.nupkg");
                        
                        bool success;
                        using (var packageStream = File.Create(dependencyPackagePath))
                        {
                            success = await resource.CopyNupkgToStreamAsync(
                                dependency.Id,
                                versionToDownload,
                                packageStream,
                                cache,
                                NullLogger.Instance,
                                CancellationToken.None);
                        }

                        if (success)
                        {
                            // Extract dependency DLLs to the lib folder
                            using var depStream = File.OpenRead(dependencyPackagePath);
                            using var depReader = new PackageArchiveReader(depStream);
                            var depFiles = await depReader.GetFilesAsync(CancellationToken.None);
                            
                            // Get the best matching lib folder for the target framework
                            var libFiles = depFiles.Where(f => f.StartsWith("lib/", StringComparison.OrdinalIgnoreCase) && f.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)).ToList();
                            
                            // Prefer netstandard2.1 > netstandard2.0 > first available
                            var bestFramework = libFiles
                                .Select(f => f.Split('/')[1])
                                .Distinct()
                                .OrderByDescending(fw => fw.Equals("netstandard2.1", StringComparison.OrdinalIgnoreCase))
                                .ThenByDescending(fw => fw.Equals("netstandard2.0", StringComparison.OrdinalIgnoreCase))
                                .FirstOrDefault();

                            int extractedDllCount = 0;
                            if (bestFramework != null)
                            {
                                var frameworkFiles = libFiles.Where(f => f.StartsWith($"lib/{bestFramework}/", StringComparison.OrdinalIgnoreCase));
                                foreach (var file in frameworkFiles)
                                {
                                    var targetPath = Path.Combine(extractPath, file);
                                    var targetDir = Path.GetDirectoryName(targetPath);
                                    if (targetDir != null && !Directory.Exists(targetDir))
                                    {
                                        Directory.CreateDirectory(targetDir);
                                    }

                                    // Skip if already extracted (from another dependency)
                                    if (File.Exists(targetPath))
                                        continue;

                                    using var sourceStream = depReader.GetStream(file);
                                    using var targetStream = File.Create(targetPath);
                                    await sourceStream.CopyToAsync(targetStream);
                                    extractedDllCount++;
                                }
                            }

                            Console.WriteLine($"{indent}  ✓ Downloaded dependency: {dependency.Id} v{versionToDownload} ({extractedDllCount} DLLs)");
                            downloadedPackages.Add(dependencyKey);
                            downloaded = true;

                            // Recursively download transitive dependencies
                            var depNuspecReader = await depReader.GetNuspecReaderAsync(CancellationToken.None);
                            var depDependencyGroups = depNuspecReader.GetDependencyGroups();
                            var depTargetGroup = depDependencyGroups
                                .OrderByDescending(g => g.TargetFramework.Framework == ".NETStandard" && g.TargetFramework.Version.Major == 2 && g.TargetFramework.Version.Minor == 1)
                                .ThenByDescending(g => g.TargetFramework.Framework == ".NETStandard" && g.TargetFramework.Version.Major == 2 && g.TargetFramework.Version.Minor == 0)
                                .FirstOrDefault();

                            if (depTargetGroup != null && depTargetGroup.Packages.Any())
                            {
                                await DownloadDependencyGroupAsync(depTargetGroup, dependenciesFolder, extractPath, cache, downloadedPackages, failedDependencies, depth + 1);
                            }

                            break;
                        }
                    }
                    catch (Exception ex)
                    {
                        lastError = ex;
                        // Try next source
                    }
                }
                
                if (!downloaded)
                {
                    failedDependencies.Add($"{dependency.Id} v{versionToDownload}");
                    if (depth == 0) // Only log first-level dependency failures
                    {
                        Console.WriteLine($"{indent}  ✗ Could not download dependency: {dependency.Id} v{versionToDownload}");
                    }
                }
            }
            catch (Exception ex)
            {
                failedDependencies.Add($"{dependency.Id}");
                if (depth == 0)
                {
                    Console.WriteLine($"{indent}  ✗ Error processing dependency {dependency.Id}: {ex.Message}");
                }
            }
        }
    }

    private List<string> GetAssemblyPaths(string extractPath)
    {
        var assemblies = new List<string>();
        var libFolder = Path.Combine(extractPath, "lib");

        if (Directory.Exists(libFolder))
        {
            assemblies.AddRange(Directory.GetFiles(libFolder, "*.dll", SearchOption.AllDirectories));
        }

        return assemblies;
    }

    public void Cleanup(PackageInfo packageInfo)
    {
        if (Directory.Exists(packageInfo.ExtractPath))
        {
            try
            {
                Directory.Delete(packageInfo.ExtractPath, recursive: true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
