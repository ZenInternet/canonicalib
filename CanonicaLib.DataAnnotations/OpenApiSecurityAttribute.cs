using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class OpenApiSecurityAttribute : Attribute
    {
        public string Scheme { get; set; }

        public string[] Scopes { get; set; } = new string[] { };

        public OpenApiSecurityAttribute(string scheme, string[]? scopes = null)
        {
            if (string.IsNullOrWhiteSpace(scheme))
                throw new ArgumentException("Scheme cannot be null, empty, or whitespace.", nameof(scheme));

            Scheme = scheme;
            if (scopes != null)
            {
                Scopes = scopes;
            }
        }
    }
}
