using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.Models
{
    public class AssemblyViewModel
    {
        public string AssemblyName { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public OpenApiDocument? Document { get; set; }
        public CanonicaLibOptions Options { get; set; } = new();
    }
}