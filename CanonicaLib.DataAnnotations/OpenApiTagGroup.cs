using Microsoft.OpenApi;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Zen.CanonicaLib.DataAnnotations
{
    public class OpenApiTagGroup
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }
        [JsonPropertyName("tags")]
        public IList<OpenApiTag> Tags { get; set; }

    }
}