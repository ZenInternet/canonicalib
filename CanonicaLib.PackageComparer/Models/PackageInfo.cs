namespace CanonicaLib.PackageComparer.Models;

public class PackageInfo
{
    public required string PackageId { get; set; }
    public required string Version { get; set; }
    public required string ExtractPath { get; set; }
    public required List<string> AssemblyPaths { get; set; }
}
