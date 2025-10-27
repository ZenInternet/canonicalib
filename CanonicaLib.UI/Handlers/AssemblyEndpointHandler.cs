using CanonicaLib.UI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using System.Text.Json;

namespace CanonicaLib.UI.Handlers
{
    public static class AssemblyEndpointHandler
    {
        public static async Task HandleAssemblyRequest(HttpContext context)
        {
            var discoveryService = context.RequestServices.GetRequiredService<DiscoveryService>();

            // TODO extract the 'slug' from the inbound route parameters and convert it to an assembly name.
            // Inbound slugs are in the format /my/assembly-name/is/this which would be converted to My.AssemblyName.Is.This
            var slug = context.Request.RouteValues["slug"]?.ToString() ?? string.Empty;

            var assemblyName = string.Join('.', slug
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpper(part[0]) + part.Substring(1)));

            var assembly = discoveryService.FindCanonicalAssembly(assemblyName);

            if (assembly == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Assembly '{assemblyName}' not found.");
                return;
            }

            var documentGeneratorService = context.RequestServices.GetRequiredService<DocumentGenerator>();

            var document = documentGeneratorService.GenerateDocument(assembly);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;

            using var stringWriter = new StringWriter();
            var jsonWriter = new OpenApiJsonWriter(stringWriter);
            document.SerializeAsV31(jsonWriter);
            var json = stringWriter.ToString();

            await context.Response.WriteAsync(json);
        }
    }
}