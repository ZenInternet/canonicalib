using Namotion.Reflection;
using System.Reflection;
using System.Xml.Linq;

namespace Zen.CanonicaLib.UI.Extensions
{
    internal static class XmlDocsExtensions
    {
        /// <summary>
        /// Gets the XML docs remarks for a member while preserving line breaks.
        /// Unlike <c>GetXmlDocsRemarks()</c>, which collapses whitespace,
        /// this method retains newlines so that multi-paragraph remarks
        /// render correctly as CommonMark in OpenAPI descriptions.
        /// </summary>
        public static string? GetXmlDocsRemarksPreservingLineBreaks(this MemberInfo memberInfo)
        {
            var element = memberInfo.GetXmlDocsElement();
            return ExtractRemarks(element);
        }

        /// <summary>
        /// Gets the XML docs remarks for a type while preserving line breaks.
        /// </summary>
        public static string? GetXmlDocsRemarksPreservingLineBreaks(this Type type)
        {
            var element = type.GetXmlDocsElement();
            return ExtractRemarks(element);
        }

        private static string? ExtractRemarks(XElement? element)
        {
            var remarks = element?.Element("remarks");
            if (remarks == null)
                return null;

            var lines = remarks.Value
                .Split('\n')
                .Select(line => line.TrimStart())
                .ToList();

            // Strip leading blank lines
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[0]))
                lines.RemoveAt(0);

            // Strip trailing blank lines
            while (lines.Count > 0 && string.IsNullOrWhiteSpace(lines[^1]))
                lines.RemoveAt(lines.Count - 1);

            if (lines.Count == 0)
                return null;

            return string.Join("\n", lines);
        }
    }
}
