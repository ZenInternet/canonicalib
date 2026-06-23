namespace Zen.CanonicaLib.UI.Tests.TestModels
{
    /// <summary>
    /// Generic test model mirroring the real Zen.Contract.PagedResult&lt;T&gt;, whose
    /// constructed Type.FullName is an assembly-qualified name containing backticks,
    /// brackets, commas and spaces. Used to verify schema keys stay identifier-safe.
    /// </summary>
    public class PagedResultModel<T>
    {
        public T[]? Items { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public int TotalRecords { get; set; }
    }
}
