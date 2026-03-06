using Microsoft.OpenApi;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.OpenApiExtensions
{
    internal class McpExtension : IOpenApiExtension
    {
        private readonly OpenApiMcpAttribute _mcp;

        public McpExtension(OpenApiMcpAttribute mcp)
        {
            _mcp = mcp;
        }

        public void Write(IOpenApiWriter writer, OpenApiSpecVersion specVersion)
        {
            writer.WriteStartObject();

            if (_mcp.Title != null)
            {
                writer.WriteProperty("title", _mcp.Title);
            }

            if (_mcp.Description != null)
            {
                writer.WriteProperty("description", _mcp.Description);
            }

            writer.WriteProperty("readOnly", _mcp.ReadOnly);
            writer.WriteProperty("destructive", _mcp.Destructive);
            writer.WriteProperty("idempotent", _mcp.Idempotent);
            writer.WriteProperty("openWorld", _mcp.OpenWorld);

            writer.WriteEndObject();
        }
    }
}
