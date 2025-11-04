using Microsoft.OpenApi;
using System.Collections.Generic;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    public class Library : ILibrary
    {
        public string FriendlyName => "Test > Contract";

        public IList<OpenApiTagGroup>? TagGroups => null;

        public IList<OpenApiServer>? Servers => null;

        public OpenApiLicense? License => new OpenApiLicense
        {
            Name = "Apache 2.0 License",
            Url = new System.Uri("https://www.apache.org/licenses/LICENSE-2.0.html")
        };
    }
}
