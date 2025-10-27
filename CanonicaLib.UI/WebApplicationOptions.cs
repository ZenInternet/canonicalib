namespace CanonicaLib.UI
{
    public record WebApplicationOptions
    {
        public string RootPath { get; init; } = "/canonicalib";

        public string RootNamespace { get; init; } = "";
    }
}
