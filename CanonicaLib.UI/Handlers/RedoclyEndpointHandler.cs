using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Zen.CanonicaLib.UI.Models;
using Zen.CanonicaLib.UI.Services;

namespace Zen.CanonicaLib.UI.Handlers
{
    public static class RedoclyEndpointHandler
    {
        public static async Task HandleRedoclyRequest(HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<WebApplicationOptions>() ?? new WebApplicationOptions();
            var discoveryService = context.RequestServices.GetRequiredService<DiscoveryService>();
            var razorViewEngine = context.RequestServices.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            
            // Extract the assembly slug from the route
            var slug = context.Request.RouteValues["slug"]?.ToString() ?? string.Empty;
            
            if (string.IsNullOrEmpty(slug))
            {
                context.Response.StatusCode = 400;
                await context.Response.WriteAsync("Assembly slug is required.");
                return;
            }

            // Convert slug to assembly name to validate it exists
            var assemblyName = ConvertSlugToAssemblyName(slug);
            var assembly = discoveryService.FindCanonicalAssembly(assemblyName);
            
            if (assembly == null)
            {
                context.Response.StatusCode = 404;
                await context.Response.WriteAsync($"Assembly '{assemblyName}' not found.");
                return;
            }

            // Remove root namespace prefix from title if configured
            var displayName = assemblyName;
            if (!string.IsNullOrEmpty(options.RootNamespace) && assemblyName.StartsWith(options.RootNamespace))
            {
                displayName = assemblyName.Substring(options.RootNamespace.Length).TrimStart('.');
            }

            // Build URLs
            var apiUrl = $"{context.Request.Scheme}://{context.Request.Host}{options.RootPath}{options.ApiPath}/{slug}";
            var backUrl = $"{context.Request.Scheme}://{context.Request.Host}{options.RootPath}";
            
            // Create the view model
            var model = new RedoclyViewModel
            {
                AssemblyName = assemblyName,
                DisplayName = displayName,
                ApiUrl = apiUrl,
                BackUrl = backUrl,
                Options = options
            };

            var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
            
            using var stringWriter = new StringWriter();
            
            // Try to find the Redocly view
            var viewResult = razorViewEngine.FindView(actionContext, "CanonicaLib/Redocly", false);
            
            if (!viewResult.Success)
            {
                // Try alternative view name
                viewResult = razorViewEngine.FindView(actionContext, "Redocly", false);
            }
            
            if (!viewResult.Success)
            {
                context.Response.StatusCode = 500;
                var searchedLocations = string.Join(", ", viewResult.SearchedLocations);
                await context.Response.WriteAsync($"Redocly view not found. Searched locations: {searchedLocations}");
                return;
            }

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                new ViewDataDictionary<RedoclyViewModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = model
                },
                tempDataProvider.GetTempData(context),
                stringWriter,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(stringWriter.ToString());
        }

        private static string ConvertSlugToAssemblyName(string slug)
        {
            return string.Join('.', slug
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpper(part[0]) + part.Substring(1)));
        }
    }
}