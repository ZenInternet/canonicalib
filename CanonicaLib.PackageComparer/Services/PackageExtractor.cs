using NuGet.Common;
using NuGet.Configuration;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Zen.CanonicaLib.PackageComparer.Models;

namespace Zen.CanonicaLib.PackageComparer.Services;

public class PackageExtractor
{
    private readonly IEnumerable<PackageSource> _packageSources;
    private readonly ISettings _settings;

    public PackageExtractor(string? packageSource = null)
    {
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
                var repository = Repository.Factory.GetCoreV3(source.Source);
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
                var repository = Repository.Factory.GetCoreV3(source.Source);
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

        return new PackageInfo
        {
            PackageId = packageId,
            Version = versionString,
            ExtractPath = extractPath,
            AssemblyPaths = GetAssemblyPaths(extractPath)
        };
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
