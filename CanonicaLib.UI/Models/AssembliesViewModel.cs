namespace Zen.CanonicaLib.UI.Models
{
    public class AssembliesViewModel
    {
        public string PageTitle { get; set; } = "CanonicaLib Assemblies";
        public List<AssemblyInfo> Assemblies { get; set; } = new();
        public WebApplicationOptions Options { get; set; } = new();
    }

    public class AssemblyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
    }
}