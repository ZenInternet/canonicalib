using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Zen.CanonicaLib.UI.Services;

namespace Zen.CanonicaLib.UI
{
    public static class WebApplicationBuilderExtensions
    {
        public static IServiceCollection AddCanonicaLib(this IServiceCollection services, Func<WebApplicationOptions>? optionsFactory = null)
        {
            services.AddSingleton(optionsFactory?.Invoke() ?? new WebApplicationOptions());

            services.AddTransient<DiscoveryService>();

            services.AddGenerators();

            // Add Razor services for UI rendering
            services.AddRazorPages();
            services.AddMvc().AddRazorRuntimeCompilation();

            return services;
        }

        private static IServiceCollection AddGenerators(this IServiceCollection services)
        {
            services.AddTransient<DocumentGenerator>();

            services.AddTransient<InfoGenerator>();

            services.AddTransient<PathsGenerator>();
            services.AddTransient<OperationGenerator>();
            services.AddTransient<ResponsesGenerator>();

            services.AddTransient<ComponentsGenerator>();
            services.AddTransient<SchemasGenerator>();
            services.AddTransient<SchemaGenerator>();

            return services;
        }
    }
}