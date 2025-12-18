namespace Zen.CanonicaLib.PackageComparer.Models;

public class PackageInfo
{
    public required string PackageId { get; set; }
    public required string Version { get; set; }
    public required string ExtractPath { get; set; }
    public required List<string> AssemblyPaths { get; set; }
    public List<string>? DependencyPaths { get; set; }  // Dependencies needed for assembly loading but not for comparison
    public string? TargetFramework { get; set; }
    public List<string>? AvailableFrameworks { get; set; }
}
