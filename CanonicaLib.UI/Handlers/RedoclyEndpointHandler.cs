using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zen.CanonicaLib.UI.Services;

namespace Zen.CanonicaLib.UI.Handlers
{
    public static class RedoclyEndpointHandler
    {
        public static async Task HandleRedoclyRequest(HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<WebApplicationOptions>() ?? new WebApplicationOptions();
            var discoveryService = context.RequestServices.GetRequiredService<DiscoveryService>();
            
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

            // Build the OpenAPI JSON URL for this assembly
            var apiUrl = $"{context.Request.Scheme}://{context.Request.Host}{options.RootPath}{options.ApiPath}/{slug}";
            
            // Generate Redocly HTML
            var html = GenerateRedoclyHtml(assemblyName, apiUrl, options);
            
            context.Response.ContentType = "text/html";
            await context.Response.WriteAsync(html);
        }

        private static string ConvertSlugToAssemblyName(string slug)
        {
            return string.Join('.', slug
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => char.ToUpper(part[0]) + part.Substring(1)));
        }

        private static string GenerateRedoclyHtml(string assemblyName, string apiUrl, WebApplicationOptions options)
        {
            // Remove root namespace prefix from title if configured
            var displayName = assemblyName;
            if (!string.IsNullOrEmpty(options.RootNamespace) && assemblyName.StartsWith(options.RootNamespace))
            {
                displayName = assemblyName.Substring(options.RootNamespace.Length).TrimStart('.');
            }

            return $@"
<!DOCTYPE html>
<html>
<head>
    <title>{displayName} - API Documentation</title>
    <meta charset=""utf-8""/>
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <link href=""https://fonts.googleapis.com/css?family=Montserrat:300,400,700|Roboto:300,400,700"" rel=""stylesheet"">
    <style>
        body {{ margin: 0; padding: 0; }}
        .loading {{ 
            display: flex; 
            justify-content: center; 
            align-items: center; 
            height: 100vh; 
            font-family: 'Roboto', sans-serif;
            color: #666;
        }}
    </style>
</head>
<body>
    <div id=""redoc-container"">
        <div class=""loading"">Loading API Documentation...</div>
    </div>
    
    <script src=""https://cdn.redoc.ly/redoc/latest/bundles/redoc.standalone.js""></script>
    <script>
        Redoc.init('{apiUrl}', {{
            scrollYOffset: 50,
            theme: {{
                colors: {{
                    primary: {{
                        main: '#667eea'
                    }}
                }},
                typography: {{
                    fontSize: '14px',
                    lineHeight: '1.5em',
                    code: {{
                        fontSize: '13px',
                        fontFamily: 'Courier, monospace'
                    }},
                    headings: {{
                        fontFamily: 'Montserrat, sans-serif',
                        fontWeight: '400'
                    }}
                }},
                sidebar: {{
                    width: '260px'
                }}
            }}
        }}, document.getElementById('redoc-container'));
    </script>
</body>
</html>";
        }
    }
}