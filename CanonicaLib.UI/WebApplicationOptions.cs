namespace Zen.CanonicaLib.UI
{
    /// <summary>
    /// Configuration options for CanonicaLib
    /// </summary>
    public record WebApplicationOptions
    {
        public string PageTitle { get; set; } = "CanonicaLib Assemblies";
        /// <summary>
        /// The root path of the canonicalib UI implementation
        /// </summary>
        public string RootPath { get; init; } = "/canonicalib";

        /// <summary>
        /// The path (relative to the root path) where the json results are available
        /// </summary>
        public string ApiPath { get; init; } = "/api";


        /// <summary>
        /// The root namespace of canonical libraries, if supplied, we'll remove this prefix from URL's and Titles in UI
        /// </summary>
        public string RootNamespace { get; init; } = "";

    }
}
