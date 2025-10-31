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

        internal static string ToFriendlyName(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            // Split by uppercase letters
            var words = System.Text.RegularExpressions.Regex.Matches(input, @"[A-Z][a-z]*|[0-9]+")
                .Select(m => m.Value);
            return string.Join(" ", words);
        }
    }
}
