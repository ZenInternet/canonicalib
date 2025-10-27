using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanonicaLib.UI.Extensions
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
