using System;

namespace Zen.CanonicaLib.DataAnnotations
{
    [AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
    public class OpenApiWebhookAttribute : Attribute
    {
        public string Purpose { get; set; }
        public OpenApiWebhookAttribute(string purpose)
        {
            Purpose = purpose;
        }
    }
}
