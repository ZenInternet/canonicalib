using Microsoft.Extensions.Logging;
using Microsoft.OpenApi;
using Zen.CanonicaLib.UI.Services.Interfaces;

namespace Zen.CanonicaLib.UI.Services
{
  public class DefaultSecurityGenerator : ISecurityGenerator
  {
    private readonly IDiscoveryService _discoveryService;
    private readonly ILogger<DefaultSecurityGenerator> _logger;

    public DefaultSecurityGenerator(
      IDiscoveryService discoveryService,
      ILogger<DefaultSecurityGenerator> logger)
    {
      _discoveryService = discoveryService ?? throw new ArgumentNullException(nameof(discoveryService));
      _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public IDictionary<string, IOpenApiSecurityScheme> GenerateSecuritySchemes(GeneratorContext generatorContext)
    {
      var secureService = _discoveryService.GetSecureServiceInstance(generatorContext.Assembly);

      return secureService?.SecuritySchemes ?? new Dictionary<string, IOpenApiSecurityScheme>();
    }
  }
}