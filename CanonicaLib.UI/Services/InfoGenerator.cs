using Microsoft.OpenApi;
using System.Reflection;

namespace Zen.CanonicaLib.UI.Services
{
    public class InfoGenerator
    {
        public OpenApiInfo GenerateInfo(Assembly assembly)
        {
            var info = new OpenApiInfo
            {
                Title = assembly.GetName().Name,
                Version = assembly.GetName().Version?.ToString() ?? "0.0.0.0",
                Description = assembly.GetCustomAttribute<AssemblyDescriptionAttribute>()?.Description ?? "No description available.",
            };
            return info;
        }
    }
}
