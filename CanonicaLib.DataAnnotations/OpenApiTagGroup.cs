using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Microsoft.OpenApi;

namespace Zen.CanonicaLib.DataAnnotations
{
    /// <summary>
    /// Represents a group of related OpenAPI tags for documentation organization.
    /// </summary>
    /// <example>
    /// <code>
    /// var tagGroup = new OpenApiTagGroup("User Management")
    /// {
    ///     Tags = 
    ///     {
    ///         new OpenApiTag { Name = "Users", Description = "User operations" },
    ///         new OpenApiTag { Name = "Roles", Description = "Role management" }
    ///     }
    /// };
    /// </code>
    /// </example>
    public sealed class OpenApiTagGroup
    {
        private string _name = string.Empty;

        /// <summary>
        /// Gets or sets the name of the tag group.
        /// </summary>
        /// <value>The name of the tag group.</value>
        /// <exception cref="ArgumentException">Thrown when the value is null, empty, or whitespace.</exception>
        [JsonPropertyName("name")]
        public string Name 
        { 
            get => _name;
            set 
            {
                if (string.IsNullOrWhiteSpace(value))
                    throw new ArgumentException("Name cannot be null, empty, or whitespace.", nameof(value));
                _name = value;
            }
        }

        /// <summary>
        /// Gets the collection of tags in this group.
        /// </summary>
        /// <value>The collection of OpenAPI tags.</value>
        [JsonPropertyName("tags")]
        public IList<OpenApiTag> Tags { get; set; } = new List<OpenApiTag>();

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiTagGroup"/> class.
        /// </summary>
        public OpenApiTagGroup()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenApiTagGroup"/> class with the specified name.
        /// </summary>
        /// <param name="name">The name of the tag group.</param>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is null, empty, or whitespace.</exception>
        public OpenApiTagGroup(string name)
        {
            Name = name; // Uses the property setter for validation
        }
    }
}