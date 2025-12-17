using System.Reflection;
using System.Runtime.Loader;
using Zen.CanonicaLib.PackageComparer.Models;

namespace Zen.CanonicaLib.PackageComparer.Services;

public class AssemblyAnalyzer
{
    public AssemblyComparison ComparePackages(PackageInfo package1, PackageInfo package2)
    {
        var package1Assemblies = AnalyzeAssemblies(package1.AssemblyPaths, "Package1");
        var package2Assemblies = AnalyzeAssemblies(package2.AssemblyPaths, "Package2");

        var typeComparisons = CompareTypes(package1Assemblies, package2Assemblies);

        return new AssemblyComparison
        {
            Package1Name = $"{package1.PackageId} {package1.Version}",
            Package2Name = $"{package2.PackageId} {package2.Version}",
            Package1Assemblies = package1Assemblies,
            Package2Assemblies = package2Assemblies,
            TypeComparisons = typeComparisons
        };
    }

    private List<AssemblyInfo> AnalyzeAssemblies(List<string> assemblyPaths, string contextName)
    {
        var assemblies = new List<AssemblyInfo>();
        
        // Create a separate AssemblyLoadContext to avoid assembly conflicts
        var loadContext = new AssemblyLoadContext(contextName, isCollectible: true);

        try
        {
            foreach (var assemblyPath in assemblyPaths)
            {
                try
                {
                    var assembly = loadContext.LoadFromAssemblyPath(assemblyPath);
                    var types = new List<Models.TypeInfo>();

                    foreach (var type in assembly.GetExportedTypes())
                    {
                        types.Add(AnalyzeType(type));
                    }

                    assemblies.Add(new AssemblyInfo
                    {
                        Name = assembly.GetName().Name ?? "Unknown",
                        Version = assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                        Path = assemblyPath,
                        Types = types
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Warning: Could not load assembly {assemblyPath}: {ex.Message}");
                }
            }
        }
        finally
        {
            // Unload the context to free resources
            loadContext.Unload();
        }

        return assemblies;
    }

    private Models.TypeInfo AnalyzeType(Type type)
    {
        var members = new List<Models.MemberInfo>();

        // Analyze methods
        foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            if (!method.IsSpecialName) // Skip property/event accessors
            {
                members.Add(new Models.MemberInfo
                {
                    Name = method.Name,
                    Kind = "Method",
                    Signature = GetMethodSignature(method),
                    IsPublic = method.IsPublic,
                    Attributes = GetAttributes(method)
                });
            }
        }

        // Analyze properties
        foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            members.Add(new Models.MemberInfo
            {
                Name = property.Name,
                Kind = "Property",
                Signature = $"{property.PropertyType.Name} {property.Name}",
                IsPublic = true,
                Attributes = GetAttributes(property)
            });
        }

        // Analyze fields
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            members.Add(new Models.MemberInfo
            {
                Name = field.Name,
                Kind = "Field",
                Signature = $"{field.FieldType.Name} {field.Name}",
                IsPublic = field.IsPublic,
                Attributes = GetAttributes(field)
            });
        }

        // Analyze events
        foreach (var evt in type.GetEvents(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            members.Add(new Models.MemberInfo
            {
                Name = evt.Name,
                Kind = "Event",
                Signature = $"event {evt.EventHandlerType?.Name} {evt.Name}",
                IsPublic = true,
                Attributes = GetAttributes(evt)
            });
        }

        // Analyze constructors
        foreach (var ctor in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
        {
            members.Add(new Models.MemberInfo
            {
                Name = ".ctor",
                Kind = "Constructor",
                Signature = GetMethodSignature(ctor),
                IsPublic = ctor.IsPublic,
                Attributes = GetAttributes(ctor)
            });
        }

        return new Models.TypeInfo
        {
            FullName = type.FullName ?? type.Name,
            Kind = GetTypeKind(type),
            IsPublic = type.IsPublic || type.IsNestedPublic,
            Members = members,
            Attributes = GetAttributes(type)
        };
    }

    private string GetTypeKind(Type type)
    {
        if (type.IsInterface) return "Interface";
        if (type.IsEnum) return "Enum";
        if (type.IsValueType) return "Struct";
        if (typeof(Delegate).IsAssignableFrom(type)) return "Delegate";
        return "Class";
    }

    private string GetMethodSignature(MethodBase method)
    {
        var parameters = string.Join(", ", method.GetParameters().Select(p => $"{p.ParameterType.Name} {p.Name}"));
        
        if (method is MethodInfo mi)
        {
            return $"{mi.ReturnType.Name} {method.Name}({parameters})";
        }
        
        return $"{method.DeclaringType?.Name}({parameters})";
    }

    private List<Models.AttributeInfo> GetAttributes(ICustomAttributeProvider member)
    {
        var attributes = new List<Models.AttributeInfo>();
        
        try
        {
            var customAttributes = member.GetCustomAttributes(inherit: false);
            
            foreach (var attr in customAttributes)
            {
                var attrType = attr.GetType();
                var attrName = attrType.Name;
                
                // Remove "Attribute" suffix for cleaner display
                if (attrName.EndsWith("Attribute"))
                {
                    attrName = attrName.Substring(0, attrName.Length - 9);
                }

                var arguments = new List<string>();
                
                // Try to extract constructor arguments
                try
                {
                    // Get properties with non-default values
                    var properties = attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                        .Where(p => p.CanRead && p.DeclaringType != typeof(Attribute));
                    
                    foreach (var prop in properties)
                    {
                        try
                        {
                            var value = prop.GetValue(attr);
                            if (value != null)
                            {
                                // Format value based on type
                                var valueStr = value switch
                                {
                                    string s => $"\"{s}\"",
                                    bool b => b.ToString().ToLower(),
                                    _ => value.ToString()
                                };
                                arguments.Add($"{prop.Name} = {valueStr}");
                            }
                        }
                        catch
                        {
                            // Skip properties that can't be read
                        }
                    }
                }
                catch
                {
                    // If we can't extract arguments, that's okay
                }

                attributes.Add(new Models.AttributeInfo
                {
                    Name = attrName,
                    Arguments = arguments
                });
            }
        }
        catch
        {
            // If we can't get attributes, return empty list
        }

        return attributes;
    }

    private List<TypeComparison> CompareTypes(List<AssemblyInfo> assemblies1, List<AssemblyInfo> assemblies2)
    {
        var comparisons = new List<TypeComparison>();

        // Create dictionaries for quick lookup, handling duplicates by grouping
        var types1 = assemblies1.SelectMany(a => a.Types)
            .GroupBy(t => t.FullName)
            .ToDictionary(g => g.Key, g => g.First());
        var types2 = assemblies2.SelectMany(a => a.Types)
            .GroupBy(t => t.FullName)
            .ToDictionary(g => g.Key, g => g.First());

        // Find all unique type names
        var allTypeNames = types1.Keys.Union(types2.Keys).Distinct().OrderBy(n => n);

        // Track types that have already been matched (for namespace changes)
        var matchedTypes1 = new HashSet<string>();
        var matchedTypes2 = new HashSet<string>();

        foreach (var typeName in allTypeNames)
        {
            var hasType1 = types1.TryGetValue(typeName, out var type1);
            var hasType2 = types2.TryGetValue(typeName, out var type2);

            ComparisonStatus status;
            List<MemberDifference> differences = new();
            bool isNamespaceChange = false;
            string? oldNamespace = null;
            string? newNamespace = null;

            if (hasType1 && hasType2)
            {
                differences = CompareMembers(type1!, type2!);
                status = differences.Count == 0 ? ComparisonStatus.Identical : ComparisonStatus.Modified;
                matchedTypes1.Add(typeName);
                matchedTypes2.Add(typeName);
            }
            else if (hasType1)
            {
                status = ComparisonStatus.OnlyInPackage1;
                // Don't add to matchedTypes - let namespace detection check these
            }
            else
            {
                status = ComparisonStatus.OnlyInPackage2;
                // Don't add to matchedTypes - let namespace detection check these
            }

            comparisons.Add(new TypeComparison
            {
                TypeName = typeName,
                Status = status,
                Package1Type = type1,
                Package2Type = type2,
                Differences = differences,
                IsNamespaceChange = isNamespaceChange,
                OldNamespace = oldNamespace,
                NewNamespace = newNamespace
            });
        }

        // Detect namespace changes by finding types with same simple name and members
        DetectNamespaceChanges(comparisons, types1, types2, matchedTypes1, matchedTypes2);

        return comparisons;
    }

    private void DetectNamespaceChanges(
        List<TypeComparison> comparisons,
        Dictionary<string, Models.TypeInfo> types1,
        Dictionary<string, Models.TypeInfo> types2,
        HashSet<string> matchedTypes1,
        HashSet<string> matchedTypes2)
    {
        // Get unmatched types from both packages
        var unmatchedTypes1 = types1.Where(kvp => !matchedTypes1.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();
        var unmatchedTypes2 = types2.Where(kvp => !matchedTypes2.Contains(kvp.Key)).Select(kvp => kvp.Value).ToList();

        foreach (var type1 in unmatchedTypes1)
        {
            // Look for a type in package2 with same simple name, kind, and similar members
            var potentialMatch = unmatchedTypes2.FirstOrDefault(type2 =>
                type2.SimpleName == type1.SimpleName &&
                type2.Kind == type1.Kind &&
                AreMembersSimilar(type1, type2));

            if (potentialMatch != null)
            {
                // Found a namespace change!
                var differences = CompareMembers(type1, potentialMatch);
                
                // Update the existing comparisons
                var comparison1 = comparisons.First(c => c.TypeName == type1.FullName);
                comparison1.Status = ComparisonStatus.NamespaceChanged;
                comparison1.Package2Type = potentialMatch;
                comparison1.IsNamespaceChange = true;
                comparison1.OldNamespace = type1.Namespace;
                comparison1.NewNamespace = potentialMatch.Namespace;
                comparison1.Differences = differences;

                // Mark the type2 comparison as matched
                var comparison2 = comparisons.First(c => c.TypeName == potentialMatch.FullName);
                comparison2.Status = ComparisonStatus.NamespaceChanged;
                comparison2.Package1Type = type1;
                comparison2.IsNamespaceChange = true;
                comparison2.OldNamespace = type1.Namespace;
                comparison2.NewNamespace = potentialMatch.Namespace;
                comparison2.Differences = differences;

                // Remove from unmatched list
                unmatchedTypes2.Remove(potentialMatch);
            }
        }
    }

    private bool AreMembersSimilar(Models.TypeInfo type1, Models.TypeInfo type2)
    {
        // For enums, just compare member names (field names) since signatures include the enum type
        if (type1.Kind == "Enum" && type2.Kind == "Enum")
        {
            var names1 = new HashSet<string>(type1.Members.Select(m => m.Name));
            var names2 = new HashSet<string>(type2.Members.Select(m => m.Name));
            
            var commonNames = names1.Intersect(names2).Count();
            var totalNames = Math.Max(names1.Count, names2.Count);
            
            return totalNames > 0 && (commonNames / (double)totalNames) >= 0.8;
        }
        
        // Consider types similar if they have at least 80% of the same member signatures
        if (type1.Members.Count == 0 && type2.Members.Count == 0)
            return true;

        var signatures1 = new HashSet<string>(type1.Members.Select(m => m.Signature));
        var signatures2 = new HashSet<string>(type2.Members.Select(m => m.Signature));

        var commonSignatures = signatures1.Intersect(signatures2).Count();
        var totalSignatures = Math.Max(signatures1.Count, signatures2.Count);

        return totalSignatures > 0 && (commonSignatures / (double)totalSignatures) >= 0.8;
    }

    private List<MemberDifference> CompareMembers(Models.TypeInfo type1, Models.TypeInfo type2)
    {
        var differences = new List<MemberDifference>();

        // Use signature as key to handle method overloads
        var members1 = type1.Members
            .GroupBy(m => m.Signature)
            .ToDictionary(g => g.Key, g => g.First());
        var members2 = type2.Members
            .GroupBy(m => m.Signature)
            .ToDictionary(g => g.Key, g => g.First());

        var allMemberKeys = members1.Keys.Union(members2.Keys).Distinct();

        foreach (var key in allMemberKeys)
        {
            var hasMember1 = members1.TryGetValue(key, out var member1);
            var hasMember2 = members2.TryGetValue(key, out var member2);

            if (hasMember1 && hasMember2)
            {
                if (member1!.Signature != member2!.Signature)
                {
                    differences.Add(new MemberDifference
                    {
                        MemberName = member1.Name,
                        Kind = DifferenceKind.SignatureChanged,
                        Details = $"Was: {member1.Signature}\nNow: {member2.Signature}"
                    });
                }
                else if (member1.IsPublic != member2.IsPublic)
                {
                    differences.Add(new MemberDifference
                    {
                        MemberName = member1.Name,
                        Kind = DifferenceKind.AccessibilityChanged,
                        Details = $"Accessibility changed from {(member1.IsPublic ? "public" : "non-public")} to {(member2.IsPublic ? "public" : "non-public")}"
                    });
                }
                
                // Compare attributes
                var attrDifferences = CompareAttributes(member1, member2);
                differences.AddRange(attrDifferences);
            }
            else if (hasMember1)
            {
                differences.Add(new MemberDifference
                {
                    MemberName = member1!.Name,
                    Kind = DifferenceKind.Removed,
                    Details = member1.Signature
                });
            }
            else
            {
                differences.Add(new MemberDifference
                {
                    MemberName = member2!.Name,
                    Kind = DifferenceKind.Added,
                    Details = member2.Signature
                });
            }
        }

        return differences;
    }

    private List<MemberDifference> CompareAttributes(Models.MemberInfo member1, Models.MemberInfo member2)
    {
        var differences = new List<MemberDifference>();

        var attrs1 = member1.Attributes.ToDictionary(a => a.Name);
        var attrs2 = member2.Attributes.ToDictionary(a => a.Name);

        var allAttrNames = attrs1.Keys.Union(attrs2.Keys).Distinct();

        foreach (var attrName in allAttrNames)
        {
            var hasAttr1 = attrs1.TryGetValue(attrName, out var attr1);
            var hasAttr2 = attrs2.TryGetValue(attrName, out var attr2);

            if (hasAttr1 && hasAttr2)
            {
                // Check if arguments changed
                var args1Str = string.Join(", ", attr1!.Arguments.OrderBy(a => a));
                var args2Str = string.Join(", ", attr2!.Arguments.OrderBy(a => a));
                
                if (args1Str != args2Str)
                {
                    differences.Add(new MemberDifference
                    {
                        MemberName = member1.Name,
                        Kind = DifferenceKind.AttributeChanged,
                        Details = $"Attribute [{attrName}] changed:\nWas: [{attrName}({args1Str})]\nNow: [{attrName}({args2Str})]"
                    });
                }
            }
            else if (hasAttr1)
            {
                differences.Add(new MemberDifference
                {
                    MemberName = member1.Name,
                    Kind = DifferenceKind.AttributeRemoved,
                    Details = $"Attribute removed: {attr1!}"
                });
            }
            else
            {
                differences.Add(new MemberDifference
                {
                    MemberName = member2.Name,
                    Kind = DifferenceKind.AttributeAdded,
                    Details = $"Attribute added: {attr2!}"
                });
            }
        }

        return differences;
    }
}
