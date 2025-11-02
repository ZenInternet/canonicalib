using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.OpenApiExtensions
{
    public class TagGroupsExtension : IOpenApiExtension
    {
        private readonly IList<OpenApiTagGroup> TagGroups;

        public TagGroupsExtension(IList<OpenApiTagGroup> tagGroups)
        {
            TagGroups = tagGroups;
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartArray();
            foreach (var tagGroup in TagGroups)
            {
                writer.WriteStartObject();

                writer.WriteProperty("name", tagGroup.Name);

                writer.WritePropertyName("tags");
                writer.WriteStartArray();
                foreach (var tag in tagGroup.Tags)
                {
                    writer.WriteValue(tag!.Name!);
                }
                writer.WriteEndArray();

                writer.WriteEndObject();
            }
            writer.WriteEndArray();
        }
    }
}
