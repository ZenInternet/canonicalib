
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    [OpenApiTag("Models")]
    public class User
    {
        [JsonPropertyName("id")]
        [Required]
        public Guid Id { get; set; }

        [JsonPropertyName("firstName")]
        [Required]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        [Required]
        public string LastName { get; set; }
    }
}
