namespace Zen.CanonicaLib.UI.Models
{
    public class RedoclyViewModel
    {
        public string AssemblyName { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string ApiUrl { get; set; } = string.Empty;
        public string BackUrl { get; set; } = string.Empty;
        public CanonicaLibOptions Options { get; set; } = new();
    }
}