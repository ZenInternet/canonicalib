using System.Text;
using System.Text.Json;
using Zen.CanonicaLib.PackageComparer.Models;

namespace Zen.CanonicaLib.PackageComparer.Services;

public class ComparisonReporter
{
    public string GenerateReport(AssemblyComparison comparison, string format, bool verbose)
    {
        return format.ToLowerInvariant() switch
        {
            "json" => GenerateJsonReport(comparison, verbose),
            "markdown" => GenerateMarkdownReport(comparison, verbose),
            _ => GenerateTextReport(comparison, verbose)
        };
    }

    private string GenerateTextReport(AssemblyComparison comparison, bool verbose)
    {
        var sb = new StringBuilder();

        sb.AppendLine("╔══════════════════════════════════════════════════════════════════════╗");
        sb.AppendLine("║          PACKAGE COMPARISON - GAP ANALYSIS REPORT                    ║");
        sb.AppendLine("╚══════════════════════════════════════════════════════════════════════╝");
        sb.AppendLine();
        sb.AppendLine($"Package 1: {comparison.Package1Name}");
        sb.AppendLine($"Package 2: {comparison.Package2Name}");
        sb.AppendLine();

        // Summary
        var identical = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.Identical);
        var modified = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.Modified);
        var onlyIn1 = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.OnlyInPackage1);
        var onlyIn2 = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.OnlyInPackage2);
        var namespaceChanged = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.NamespaceChanged);

        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("SUMMARY");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"  Total Types Analyzed: {comparison.TypeComparisons.Count}");
        sb.AppendLine($"  Identical Types:      {identical}");
        sb.AppendLine($"  Modified Types:       {modified}");
        sb.AppendLine($"  Namespace Changed:    {namespaceChanged / 2}"); // Divided by 2 because each change creates 2 entries
        sb.AppendLine($"  Only in Package 1:    {onlyIn1}");
        sb.AppendLine($"  Only in Package 2:    {onlyIn2}");
        sb.AppendLine();

        // Assemblies
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("ASSEMBLIES");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine($"Package 1 Assemblies ({comparison.Package1Assemblies.Count}):");
        foreach (var asm in comparison.Package1Assemblies)
        {
            sb.AppendLine($"  • {asm.Name} v{asm.Version} ({asm.Types.Count} public types)");
        }
        sb.AppendLine();
        sb.AppendLine($"Package 2 Assemblies ({comparison.Package2Assemblies.Count}):");
        foreach (var asm in comparison.Package2Assemblies)
        {
            sb.AppendLine($"  • {asm.Name} v{asm.Version} ({asm.Types.Count} public types)");
        }
        sb.AppendLine();

        // Types removed (gap in package 2)
        if (onlyIn1 > 0)
        {
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("⚠ TYPES REMOVED (Present in Package 1, Missing in Package 2)");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            foreach (var type in comparison.TypeComparisons.Where(t => t.Status == ComparisonStatus.OnlyInPackage1))
            {
                sb.AppendLine($"  • {type.Package1Type!.Kind}: {type.TypeName}");
                if (verbose && type.Package1Type.Members.Count > 0)
                {
                    sb.AppendLine($"    Members: {type.Package1Type.Members.Count}");
                }
            }
            sb.AppendLine();
        }

        // Types added (new in package 2)
        if (onlyIn2 > 0)
        {
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("✓ TYPES ADDED (New in Package 2)");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            foreach (var type in comparison.TypeComparisons.Where(t => t.Status == ComparisonStatus.OnlyInPackage2))
            {
                sb.AppendLine($"  • {type.Package2Type!.Kind}: {type.TypeName}");
                if (verbose && type.Package2Type.Members.Count > 0)
                {
                    sb.AppendLine($"    Members: {type.Package2Type.Members.Count}");
                }
            }
            sb.AppendLine();
        }

        // Namespace changes
        var namespaceChanges = comparison.TypeComparisons
            .Where(t => t.Status == ComparisonStatus.NamespaceChanged && t.IsNamespaceChange)
            .GroupBy(t => t.Package1Type?.FullName ?? t.TypeName)
            .Select(g => g.First())
            .ToList();

        if (namespaceChanges.Count > 0)
        {
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("↔ NAMESPACE CHANGES");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            foreach (var type in namespaceChanges)
            {
                var type1 = type.Package1Type ?? type.Package2Type;
                var type2 = type.Package2Type ?? type.Package1Type;
                sb.AppendLine($"  {type1!.Kind}: {type1.SimpleName}");
                sb.AppendLine($"    From: {type.OldNamespace}");
                sb.AppendLine($"    To:   {type.NewNamespace}");
                if (type.Differences.Count > 0)
                {
                    sb.AppendLine($"    Additional Changes: {type.Differences.Count}");
                }
                if (verbose && type.Differences.Count > 0)
                {
                    foreach (var diff in type.Differences)
                    {
                        var symbol = diff.Kind switch
                        {
                            DifferenceKind.Added => "+",
                            DifferenceKind.Removed => "-",
                            DifferenceKind.SignatureChanged => "~",
                            DifferenceKind.AccessibilityChanged => "◊",
                            _ => "?"
                        };
                        sb.AppendLine($"      {symbol} {diff.Kind}: {diff.MemberName}");
                    }
                }
                sb.AppendLine();
            }
        }

        // Modified types (breaking changes)
        if (modified > 0)
        {
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            sb.AppendLine("⚡ TYPES MODIFIED");
            sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
            foreach (var type in comparison.TypeComparisons.Where(t => t.Status == ComparisonStatus.Modified))
            {
                sb.AppendLine($"  {type.Package1Type!.Kind}: {type.TypeName}");
                sb.AppendLine($"    Changes: {type.Differences.Count}");

                if (verbose)
                {
                    foreach (var diff in type.Differences)
                    {
                        var symbol = diff.Kind switch
                        {
                            DifferenceKind.Added => "+",
                            DifferenceKind.Removed => "-",
                            DifferenceKind.SignatureChanged => "~",
                            DifferenceKind.AccessibilityChanged => "◊",
                            DifferenceKind.AttributeAdded => "⊕",
                            DifferenceKind.AttributeRemoved => "⊖",
                            DifferenceKind.AttributeChanged => "⊙",
                            _ => "?"
                        };
                        sb.AppendLine($"      {symbol} {diff.Kind}: {diff.MemberName}");
                        if (!string.IsNullOrEmpty(diff.Details))
                        {
                            foreach (var line in diff.Details.Split('\n'))
                            {
                                sb.AppendLine($"        {line}");
                            }
                        }
                    }
                }
                sb.AppendLine();
            }
        }

        // Conclusion
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        sb.AppendLine("CONCLUSION");
        sb.AppendLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        if (onlyIn1 > 0 || modified > 0)
        {
            sb.AppendLine("⚠ Breaking changes detected! Package 2 has removed or modified types.");
        }
        else if (onlyIn2 > 0)
        {
            sb.AppendLine("✓ No breaking changes. Package 2 adds new functionality.");
        }
        else
        {
            sb.AppendLine("✓ Packages are functionally identical.");
        }

        return sb.ToString();
    }

    private string GenerateMarkdownReport(AssemblyComparison comparison, bool verbose)
    {
        var sb = new StringBuilder();

        sb.AppendLine("# Package Comparison - Gap Analysis Report");
        sb.AppendLine();
        sb.AppendLine($"**Package 1:** {comparison.Package1Name}  ");
        sb.AppendLine($"**Package 2:** {comparison.Package2Name}");
        sb.AppendLine();

        var identical = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.Identical);
        var modified = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.Modified);
        var onlyIn1 = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.OnlyInPackage1);
        var onlyIn2 = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.OnlyInPackage2);
        var namespaceChanged = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.NamespaceChanged);

        sb.AppendLine("## Summary");
        sb.AppendLine();
        sb.AppendLine("| Metric | Count |");
        sb.AppendLine("|--------|-------|");
        sb.AppendLine($"| Total Types | {comparison.TypeComparisons.Count} |");
        sb.AppendLine($"| Identical Types | {identical} |");
        sb.AppendLine($"| Modified Types | {modified} |");
        sb.AppendLine($"| Namespace Changed | {namespaceChanged / 2} |");
        sb.AppendLine($"| Only in Package 1 | {onlyIn1} |");
        sb.AppendLine($"| Only in Package 2 | {onlyIn2} |");
        sb.AppendLine();

        sb.AppendLine("## Assemblies");
        sb.AppendLine();
        sb.AppendLine($"### Package 1 ({comparison.Package1Assemblies.Count} assemblies)");
        sb.AppendLine();
        foreach (var asm in comparison.Package1Assemblies)
        {
            sb.AppendLine($"- **{asm.Name}** v{asm.Version} ({asm.Types.Count} public types)");
        }
        sb.AppendLine();
        sb.AppendLine($"### Package 2 ({comparison.Package2Assemblies.Count} assemblies)");
        sb.AppendLine();
        foreach (var asm in comparison.Package2Assemblies)
        {
            sb.AppendLine($"- **{asm.Name}** v{asm.Version} ({asm.Types.Count} public types)");
        }
        sb.AppendLine();

        if (onlyIn1 > 0)
        {
            sb.AppendLine("## ⚠️ Types Removed");
            sb.AppendLine();
            sb.AppendLine("These types are present in Package 1 but missing in Package 2:");
            sb.AppendLine();
            foreach (var type in comparison.TypeComparisons.Where(t => t.Status == ComparisonStatus.OnlyInPackage1))
            {
                sb.AppendLine($"- `{type.TypeName}` ({type.Package1Type!.Kind})");
            }
            sb.AppendLine();
        }

        if (onlyIn2 > 0)
        {
            sb.AppendLine("## ✅ Types Added");
            sb.AppendLine();
            sb.AppendLine("These types are new in Package 2:");
            sb.AppendLine();
            foreach (var type in comparison.TypeComparisons.Where(t => t.Status == ComparisonStatus.OnlyInPackage2))
            {
                sb.AppendLine($"- `{type.TypeName}` ({type.Package2Type!.Kind})");
            }
            sb.AppendLine();
        }

        var namespaceChanges = comparison.TypeComparisons
            .Where(t => t.Status == ComparisonStatus.NamespaceChanged && t.IsNamespaceChange)
            .GroupBy(t => t.Package1Type?.FullName ?? t.TypeName)
            .Select(g => g.First())
            .ToList();

        if (namespaceChanges.Count > 0)
        {
            sb.AppendLine("## 🔄 Namespace Changes");
            sb.AppendLine();
            sb.AppendLine("These types have been moved to different namespaces:");
            sb.AppendLine();
            foreach (var type in namespaceChanges)
            {
                var type1 = type.Package1Type ?? type.Package2Type;
                sb.AppendLine($"- **`{type1!.SimpleName}`** ({type1.Kind})");
                sb.AppendLine($"  - From: `{type.OldNamespace}`");
                sb.AppendLine($"  - To: `{type.NewNamespace}`");
                if (type.Differences.Count > 0)
                {
                    sb.AppendLine($"  - Additional changes: {type.Differences.Count}");
                }
            }
            sb.AppendLine();
        }

        if (modified > 0)
        {
            sb.AppendLine("## ⚡ Types Modified");
            sb.AppendLine();
            foreach (var type in comparison.TypeComparisons.Where(t => t.Status == ComparisonStatus.Modified))
            {
                sb.AppendLine($"### `{type.TypeName}`");
                sb.AppendLine();
                foreach (var diff in type.Differences)
                {
                    var emoji = diff.Kind switch
                    {
                        DifferenceKind.Added => "➕",
                        DifferenceKind.Removed => "➖",
                        DifferenceKind.SignatureChanged => "🔄",
                        DifferenceKind.AccessibilityChanged => "🔒",
                        DifferenceKind.AttributeAdded => "🏷️",
                        DifferenceKind.AttributeRemoved => "🚫",
                        DifferenceKind.AttributeChanged => "📝",
                        _ => "❓"
                    };
                    sb.AppendLine($"- {emoji} **{diff.Kind}**: `{diff.MemberName}`");
                    if (verbose && !string.IsNullOrEmpty(diff.Details))
                    {
                        sb.AppendLine($"  ```");
                        sb.AppendLine($"  {diff.Details}");
                        sb.AppendLine($"  ```");
                    }
                }
                sb.AppendLine();
            }
        }

        sb.AppendLine("## Conclusion");
        sb.AppendLine();
        if (onlyIn1 > 0 || modified > 0)
        {
            sb.AppendLine("⚠️ **Breaking changes detected!** Package 2 has removed or modified types.");
        }
        else if (onlyIn2 > 0)
        {
            sb.AppendLine("✅ **No breaking changes.** Package 2 adds new functionality.");
        }
        else
        {
            sb.AppendLine("✅ **Packages are functionally identical.**");
        }

        return sb.ToString();
    }

    private string GenerateJsonReport(AssemblyComparison comparison, bool verbose)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        if (!verbose)
        {
            var namespaceChanges = comparison.TypeComparisons
                .Where(t => t.Status == ComparisonStatus.NamespaceChanged && t.IsNamespaceChange)
                .GroupBy(t => t.Package1Type?.FullName ?? t.TypeName)
                .Select(g => g.First())
                .ToList();

            // Create simplified version without detailed member info
            var simplified = new
            {
                package1 = comparison.Package1Name,
                package2 = comparison.Package2Name,
                summary = new
                {
                    totalTypes = comparison.TypeComparisons.Count,
                    identical = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.Identical),
                    modified = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.Modified),
                    namespaceChanged = namespaceChanges.Count,
                    onlyInPackage1 = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.OnlyInPackage1),
                    onlyInPackage2 = comparison.TypeComparisons.Count(t => t.Status == ComparisonStatus.OnlyInPackage2)
                },
                namespaceChanges = namespaceChanges.Select(t => new
                {
                    typeName = (t.Package1Type ?? t.Package2Type)!.SimpleName,
                    oldNamespace = t.OldNamespace,
                    newNamespace = t.NewNamespace,
                    additionalChanges = t.Differences.Count
                }),
                differences = comparison.TypeComparisons
                    .Where(t => t.Status != ComparisonStatus.Identical && t.Status != ComparisonStatus.NamespaceChanged)
                    .Select(t => new
                    {
                        typeName = t.TypeName,
                        status = t.Status.ToString(),
                        changeCount = t.Differences.Count
                    })
            };

            return JsonSerializer.Serialize(simplified, options);
        }

        return JsonSerializer.Serialize(comparison, options);
    }
}
