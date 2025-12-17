namespace Zen.CanonicaLib.PackageComparer.Models;

public class AssemblyComparison
{
    public required string Package1Name { get; set; }
    public required string Package2Name { get; set; }
    public required List<AssemblyInfo> Package1Assemblies { get; set; }
    public required List<AssemblyInfo> Package2Assemblies { get; set; }
    public required List<TypeComparison> TypeComparisons { get; set; }
}

public class AssemblyInfo
{
    public required string Name { get; set; }
    public required string Version { get; set; }
    public required string Path { get; set; }
    public required List<TypeInfo> Types { get; set; }
}

public class TypeInfo
{
    public required string FullName { get; set; }
    public required string Kind { get; set; } // Class, Interface, Enum, Struct, Delegate
    public required bool IsPublic { get; set; }
    public required List<MemberInfo> Members { get; set; }
    public List<AttributeInfo> Attributes { get; set; } = new();
    
    public string SimpleName => FullName?.Contains('.') == true ? FullName.Substring(FullName.LastIndexOf('.') + 1) : FullName ?? string.Empty;
    public string? Namespace => FullName?.Contains('.') == true ? FullName.Substring(0, FullName.LastIndexOf('.')) : null;
}

public class MemberInfo
{
    public required string Name { get; set; }
    public required string Kind { get; set; } // Method, Property, Field, Event, Constructor
    public required string Signature { get; set; }
    public required bool IsPublic { get; set; }
    public List<AttributeInfo> Attributes { get; set; } = new();
}

public class TypeComparison
{
    public required string TypeName { get; set; }
    public required ComparisonStatus Status { get; set; }
    public TypeInfo? Package1Type { get; set; }
    public TypeInfo? Package2Type { get; set; }
    public required List<MemberDifference> Differences { get; set; }
    public bool IsNamespaceChange { get; set; }
    public string? OldNamespace { get; set; }
    public string? NewNamespace { get; set; }
}

public class MemberDifference
{
    public required string MemberName { get; set; }
    public required DifferenceKind Kind { get; set; }
    public string? Details { get; set; }
}

public enum ComparisonStatus
{
    Identical,
    Modified,
    OnlyInPackage1,
    OnlyInPackage2,
    NamespaceChanged
}

public class AttributeInfo
{
    public required string Name { get; set; }
    public List<string> Arguments { get; set; } = new();
    
    public override string ToString()
    {
        if (Arguments.Count == 0)
            return $"[{Name}]";
        return $"[{Name}({string.Join(", ", Arguments)})]";
    }
}

public enum DifferenceKind
{
    Added,
    Removed,
    SignatureChanged,
    AccessibilityChanged,
    AttributeAdded,
    AttributeRemoved,
    AttributeChanged
}
