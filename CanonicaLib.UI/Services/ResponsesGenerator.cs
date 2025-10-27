using CanonicaLib.DataAnnotations;
using Microsoft.OpenApi;
using System.Reflection;

namespace CanonicaLib.UI.Services
{
    internal class ResponsesGenerator
    {
        internal static OpenApiResponses GenerateResponses(MethodInfo endpointDefinition)
        {
            var responses = new OpenApiResponses();

            var attributes = endpointDefinition.GetCustomAttributes<ResponseAttribute>();
            foreach (var attribute in attributes)
            {
                var statusCode = attribute.StatusCode.ToString();
                var description = attribute.Description ?? string.Empty;
                var responseType = attribute.ResponseType?.ToString();
                responses[statusCode] = new OpenApiResponse
                {
                    Description = description,
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        { "application/json", new OpenApiMediaType()
                            {
                                Schema = responseType != null ? new OpenApiSchemaReference(responseType.Replace(".", "_")) : null,
                            }
                        }
                    }
                };
            }

            return responses;
        }
    }
}