using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using System.Reflection;
using Zen.CanonicaLib.UI.Services;

namespace Zen.CanonicaLib.UI
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection AddCanonicaLib(this IServiceCollection services, Func<CanonicaLibOptions>? optionsFactory = null)
        {
            services.AddSingleton(optionsFactory?.Invoke() ?? new CanonicaLibOptions());

            services.AddTransient<DiscoveryService>();

            services.AddGenerators();

            // Add Razor services for UI rendering
            services.AddRazorPages();
            services.AddMvc().AddRazorRuntimeCompilation();

            // Configure Razor to find views in the class library
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationFormats.Add("/Views/{1}/{0}.cshtml");
                options.ViewLocationFormats.Add("/Views/Shared/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/{1}/{0}.cshtml");
                options.AreaViewLocationFormats.Add("/Areas/{2}/Views/Shared/{0}.cshtml");
            });

            // Add embedded file provider for views
            services.Configure<MvcRazorRuntimeCompilationOptions>(options =>
            {
                var assembly = Assembly.GetAssembly(typeof(WebApplicationBuilderExtensions));
                if (assembly != null)
                {
                    var embeddedFileProvider = new EmbeddedFileProvider(assembly);
                    options.FileProviders.Add(embeddedFileProvider);
                }
            });

            return services;
        }

        private static IServiceCollection AddGenerators(this IServiceCollection services)
        {
            services.AddTransient<ComponentsGenerator>();
            services.AddTransient<DocumentGenerator>();
            services.AddTransient<ExamplesGenerator>();
            services.AddTransient<InfoGenerator>();
            services.AddTransient<OperationGenerator>();
            services.AddTransient<ParametersGenerator>();
            services.AddTransient<PathsGenerator>();
            services.AddTransient<RequestBodyGenerator>();
            services.AddTransient<ResponsesGenerator>();
            services.AddTransient<SchemaGenerator>();
            services.AddTransient<SchemasGenerator>();
            services.AddTransient<TagGroupsGenerator>();

            return services;
        }
    }
}