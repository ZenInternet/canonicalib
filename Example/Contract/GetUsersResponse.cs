using System.Collections.Generic;
using System.Text.Json.Serialization;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    [OpenApiTag("Models")]
    public class GetUsersResponse
    {
        [JsonPropertyName("users")]
        public IList<User> Users { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
