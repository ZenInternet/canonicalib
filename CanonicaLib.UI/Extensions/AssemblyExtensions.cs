using System.Reflection;

namespace Zen.CanonicaLib.UI.Extensions
{
    public static class AssemblyExtensions
    {
        public static string ConvertToSlug(this Assembly assembly)
        {
            return assembly.FullName!.Split(",")[0]
                .ToLowerInvariant().Replace('.', '/');
        }
    }
}
