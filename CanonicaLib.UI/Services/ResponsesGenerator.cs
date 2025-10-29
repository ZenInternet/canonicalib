using Microsoft.OpenApi;
using System.Reflection;
using Zen.CanonicaLib.DataAnnotations;

namespace Zen.CanonicaLib.UI.Services
{
    public class ResponsesGenerator
    {
        public OpenApiResponses GenerateResponses(MethodInfo endpointDefinition)
        {
            var responses = new OpenApiResponses();

            var attributes = endpointDefinition.GetCustomAttributes<ResponseAttribute>();
            foreach (var attribute in attributes)
            {
                var statusCode = attribute.StatusCode.ToString();
                var description = attribute.Description ?? string.Empty;
                var responseType = attribute.Type?.ToString();
                responses[statusCode] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType()
                            {
                                Schema = responseType != null ? new OpenApiSchemaReference(responseType) : null,
                            }
                        }
                    }
                };
            }

            return responses;
        }
    }
}