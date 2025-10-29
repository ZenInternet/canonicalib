using System.Collections.Generic;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    public class Library : ILibrary
    {
        public string FriendlyName => "Test > Contract";

        public IList<OpenApiTagGroup>? TagGroups => null;
    }
}
