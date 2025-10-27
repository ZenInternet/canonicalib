namespace Zen.CanonicaLib.UI.Extensions
{
    internal static class StringExtensions
    {
        internal static string? IfEmpty(this string input, string? defaultValue)
        {
            if (string.IsNullOrEmpty(input))
                return defaultValue;
            return input;
        }
    }
}
