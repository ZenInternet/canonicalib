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
    public static class UIEndpointHandler
    {
        public static async Task HandleUIRequest(HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<WebApplicationOptions>() ?? new WebApplicationOptions();
            var discoveryService = context.RequestServices.GetRequiredService<DiscoveryService>();
            var razorViewEngine = context.RequestServices.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();
            
            var assemblies = discoveryService.FindCanonicalAssemblies();
            
            var model = new AssembliesViewModel
            {
                Assemblies = assemblies.Select(a => new AssemblyInfo 
                { 
                    Name = a.FullName ?? "Unknown",
                    Slug = ConvertToSlug(a.FullName ?? "")
                }).ToList(),
                Options = options
            };

            var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());
            
            using var stringWriter = new StringWriter();
            var viewResult = razorViewEngine.FindView(actionContext, "Index", false);
            
            if (!viewResult.Success)
            {
                // Fallback to embedded view content
                await RenderFallbackView(context, model);
                return;
            }

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                new ViewDataDictionary<AssembliesViewModel>(new EmptyModelMetadataProvider(), new ModelStateDictionary())
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

        private static async Task RenderFallbackView(HttpContext context, AssembliesViewModel model)
        {
            var html = GenerateAssembliesHtml(model);
            
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        }

        private static string ConvertToSlug(string assemblyName)
        {
            return assemblyName.ToLowerInvariant().Replace('.', '/');
        }

        private static string ConvertSlugToAssemblyName(string slug)
        {
            return string.Join('.', slug
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpper(part[0]) + part.Substring(1)));
        }

        private static string GenerateAssembliesHtml(AssembliesViewModel model)
        {
            return $@"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>CanonicaLib Documentation</title>
    <link href=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/css/bootstrap.min.css"" rel=""stylesheet"" />
    <style>
        .assembly-card {{ transition: transform 0.2s; }}
        .assembly-card:hover {{ transform: translateY(-2px); }}
        .hero-section {{ background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; }}
    </style>
</head>
<body>
    <div class=""hero-section py-5"">
        <div class=""container"">
            <h1 class=""display-4 fw-bold mb-3"">CanonicaLib Documentation</h1>
            <p class=""lead"">Explore your canonical assemblies and their OpenAPI specifications</p>
        </div>
    </div>
    
    <div class=""container my-5"">
        <div class=""row"">
            <div class=""col-12"">
                <h2 class=""mb-4"">Available Assemblies ({model.Assemblies.Count})</h2>
                
                {(model.Assemblies.Any() ? 
                    @"<div class=""row"">" +
                    string.Join("", model.Assemblies.Select(assembly => $@"
                        <div class=""col-md-6 col-lg-4 mb-4"">
                            <div class=""card assembly-card h-100 shadow-sm"">
                                <div class=""card-body"">
                                    <h5 class=""card-title"">{assembly.Name}</h5>
                                    <p class=""card-text text-muted"">Click to explore the OpenAPI specification</p>
                                    <a href=""{model.Options.RootPath}/ui/{assembly.Slug}"" class=""btn btn-primary"">View Documentation</a>
                                </div>
                            </div>
                        </div>
                    ")) +
                    "</div>"
                    :
                    @"<div class=""alert alert-info"">
                        <h4 class=""alert-heading"">No Assemblies Found</h4>
                        <p>No canonical assemblies were discovered in the current application.</p>
                    </div>"
                )}
            </div>
        </div>
    </div>
    
    <script src=""https://cdn.jsdelivr.net/npm/bootstrap@5.3.0/dist/js/bootstrap.bundle.min.js""></script>
</body>
</html>";
        }
        private static string GetMethodBadgeClass(string method)
        {
            return method.ToLower() switch
            {
                "get" => "bg-primary",
                "post" => "bg-success",
                "put" => "bg-warning",
                "delete" => "bg-danger",
                "patch" => "bg-info",
                _ => "bg-secondary"
            };
        }
    }
}