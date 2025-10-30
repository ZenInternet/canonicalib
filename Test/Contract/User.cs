
using System;
using System.Text.Json.Serialization;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    [OpenApiTag("Models")]
    public class User
    {
        [JsonPropertyName("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }
    }
}
