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
    public static class UIEndpointHandler
    {
        public static async Task HandleUIRequest(HttpContext context)
        {
            var options = context.RequestServices.GetRequiredService<CanonicaLibOptions>() ?? new CanonicaLibOptions();
            var discoveryService = context.RequestServices.GetRequiredService<IDiscoveryService>();
            var razorViewEngine = context.RequestServices.GetRequiredService<IRazorViewEngine>();
            var tempDataProvider = context.RequestServices.GetRequiredService<ITempDataDictionaryFactory>();

            var assemblies = discoveryService.FindCanonicalAssemblies();

            var model = new AssembliesViewModel
            {
                PageTitle = options.PageTitle,
                Assemblies = assemblies.Select(a => new AssemblyInfo
                {
                    Name = a.FullName?.Split(",")[0] ?? "Unknown",
                    Slug = a.ConvertToSlug()
                }).ToList(),
                Options = options
            };

            var actionContext = new ActionContext(context, context.GetRouteData(), new ActionDescriptor());

            using var stringWriter = new StringWriter();

            // Try to find the view using the configured view locations
            var viewResult = razorViewEngine.FindView(actionContext, "CanonicaLib/Index", false);

            if (!viewResult.Success)
            {
                // Try alternative view name
                viewResult = razorViewEngine.FindView(actionContext, "Index", false);
            }

            if (!viewResult.Success)
            {
                context.Response.StatusCode = 500;
                var searchedLocations = string.Join(", ", viewResult.SearchedLocations);
                await context.Response.WriteAsync($"View not found. Searched locations: {searchedLocations}");
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

    }
}