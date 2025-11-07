using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Handlers
{
    public static class AssemblyEndpointHandler
    {
        public static async Task HandleAssemblyRequest(HttpContext context)
        {
            var discoveryService = context.RequestServices.GetRequiredService<IDiscoveryService>();

            // TODO extract the 'slug' from the inbound route parameters and convert it to an assembly name.
            // Inbound slugs are in the format /my/assembly-name/is/this which would be converted to My.AssemblyName.Is.This
            var slug = context.Request.RouteValues["slug"]?.ToString() ?? string.Empty;

            if (slug.Contains("/attachments/"))
            {
                // Redirect to attachment handler if the URL contains /attachments/
                await AttachmentEndpointHandler.HandleAttachmentRequest(context);
                return;
            }

            var assemblyName = slug.ConvertToAssemblyName();
            var assembly = discoveryService.FindCanonicalAssembly(assemblyName);

            if (assembly == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Assembly '{assemblyName}' not found.");
                return;
            }

            var documentGeneratorService = context.RequestServices.GetRequiredService<IDocumentGenerator>();

            var generatorContext = documentGeneratorService.GenerateDocument(assembly);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;

            using var stringWriter = new StringWriter();
            var jsonWriter = new OpenApiJsonWriter(stringWriter);
            generatorContext.Document.SerializeAsV31(jsonWriter);
            var json = stringWriter.ToString();

            await context.Response.WriteAsync(json);
        }
    }
}