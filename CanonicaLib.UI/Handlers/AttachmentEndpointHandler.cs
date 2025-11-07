using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zen.CanonicaLib.UI.Extensions;
using Zen.CanonicaLib.UI.Models;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Handlers
{
    public static class AttachmentEndpointHandler
    {
        public static async Task HandleAttachmentRequest(HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<CanonicaLibOptions>() ?? new CanonicaLibOptions();
            var discoveryService = context.RequestServices.GetRequiredService<IDiscoveryService>();

            // Extract the assembly slug from the route
            var slug = context.Request.RouteValues["slug"]?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(slug))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Assembly slug is required.");
                return;
            }

            if (!slug.Contains("/attachments/"))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Invalid attachment request format.");
                return;
            }

            var parts = slug.Split("/attachments/", StringSplitOptions.RemoveEmptyEntries);
            slug = parts[0];
            var attachment = parts.Length > 1 ? parts[1] : string.Empty;

            if (string.IsNullOrEmpty(attachment))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Attachment name is required.");
                return;
            }

            // Convert slug to assembly name to validate it exists
            var assemblyName = slug.ConvertToAssemblyName();
            var assembly = discoveryService.FindCanonicalAssembly(assemblyName);

            if (assembly == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Assembly '{assemblyName}' not found.");
                return;
            }

            try
            {
                var attachmentStream = discoveryService.FindAttachmentInAssembly(assembly, attachment);

                if (attachmentStream == null)
                {
                    context.Response.StatusCode = 404;
                    await context.Response.WriteAsync($"Attachment '{attachment}' not found in assembly '{assemblyName}'.");
                    return;
                }

                context.Response.ContentType = "application/octet-stream";
                await attachmentStream.CopyToAsync(context.Response.Body);
                return;

            }
            catch (FileNotFoundException ex)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Attachment '{attachment}' not found in assembly '{assemblyName}'. {ex.Message}");
                return;
            }
            catch (InvalidOperationException ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Error retrieving attachment '{attachment}': {ex.Message}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = 500;
                await context.Response.WriteAsync($"Unexpected error: {ex.Message}");
                return;
            }
        }
    }
}