using System.Reflection;

namespace Zen.CanonicaLib.UI.Extensions
{
    public static class AssemblyExtensions
    {
        public static string ConvertToSlug(this Assembly assembly)
        {
            return assembly.FullName!.Split(",")[0]
                .ToLowerInvariant().Replace('.', '/') + "/";
        }

        public static string ConvertToAssemblyName(this string slug)
        {
            return string.Join('.', slug
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpper(part[0]) + part.Substring(1)));
        }
    }
}
