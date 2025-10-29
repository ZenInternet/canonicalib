using Microsoft.OpenApi;

namespace Zen.CanonicaLib.UI.OpenApiExtensions
{
    public class TagsExtension : IOpenApiExtension
    {
        private readonly IList<string> Tags;

        public TagsExtension(IList<string> tags)
        {
            Tags = tags;
        }

        public TagsExtension(string tag)
        {
            Tags = new List<string> { tag };
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartArray();
            foreach (var tag in Tags)
            {
                writer.WriteValue(tag);
            }
            writer.WriteEndArray();
        }
    }
}
