using Microsoft.OpenApi;
using System.Collections.Generic;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    public class Library : ILibrary, ISecureService, IService
    {
        public string FriendlyName => "Test > Contract";

        public IList<OpenApiTagGroup>? TagGroups => null;


        public OpenApiLicense? License => new OpenApiLicense
        {
            Name = "Apache 2.0 License",
            Url = new System.Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
        };

        public IList<OpenApiServer>? Servers => null;
        public IDictionary<string, IOpenApiSecurityScheme>? SecuritySchemes => new Dictionary<string, IOpenApiSecurityScheme>
        {
            {
                "oauth2",
                new OpenApiSecurityScheme
                {
                    Name = "O Auth 2",
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        AuthorizationCode = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new System.Uri("https://example.com/oauth2/authorize"),
                            TokenUrl = new System.Uri("https://example.com/oauth2/token"),
                            Scopes = new Dictionary<string, string>
                            {
                                { "pre-sales", "Perform pre-sales activities" },
                                { "billing", "Access billing features" },
                                { "user-management", "Manage user accounts and permissions" }
                            }
                        }
                    }
                }
            }
        };
    }
}
