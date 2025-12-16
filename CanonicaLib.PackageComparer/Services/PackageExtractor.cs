using NuGet.Common;
using NuGet.Packaging;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using CanonicaLib.PackageComparer.Models;

namespace CanonicaLib.PackageComparer.Services;

public class PackageExtractor
{
    private readonly string? _packageSource;

    public PackageExtractor(string? packageSource = null)
    {
        _packageSource = packageSource ?? "https://api.nuget.org/v3/index.json";
    }

    public async Task<PackageInfo> ExtractPackageAsync(string packageIdentifier)
    {
        // Check if it's a file path
        if (File.Exists(packageIdentifier) && packageIdentifier.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase))
        {
            return await ExtractLocalPackageAsync(packageIdentifier);
        }

        // Parse package ID and version from format: PackageId/Version
        var parts = packageIdentifier.Split('/');
        if (parts.Length != 2)
        {
            throw new ArgumentException("Package identifier must be in format 'PackageId/Version' or a path to .nupkg file");
        }

        var packageId = parts[0];
        var version = parts[1];

        return await DownloadAndExtractPackageAsync(packageId, version);
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
        var repository = Repository.Factory.GetCoreV3(_packageSource!);
        var resource = await repository.GetResourceAsync<FindPackageByIdResource>();

        if (!NuGetVersion.TryParse(versionString, out var nugetVersion))
        {
            throw new ArgumentException($"Invalid version format: {versionString}");
        }

        var extractPath = Path.Combine(Path.GetTempPath(), $"pkg_compare_{Guid.NewGuid()}");
        Directory.CreateDirectory(extractPath);

        var packagePath = Path.Combine(extractPath, $"{packageId}.{versionString}.nupkg");

        using (var packageStream = File.Create(packagePath))
        {
            var success = await resource.CopyNupkgToStreamAsync(
                packageId,
                nugetVersion,
                packageStream,
                cache,
                NullLogger.Instance,
                CancellationToken.None);

            if (!success)
            {
                throw new Exception($"Failed to download package {packageId}/{versionString}");
            }
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
