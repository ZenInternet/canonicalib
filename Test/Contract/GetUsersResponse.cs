using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Test.Contract
{
    public class GetUsersResponse
    {
        [JsonPropertyName("users")]
        public IList<User> Users { get; set; }

        [JsonPropertyName("count")]
        public int Count { get; set; }
    }
}
