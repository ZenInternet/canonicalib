using System;
using System.Collections.Generic;
using System.Text;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    public class Library : ILibrary
    {
        public string FriendlyName => "Test > Contract";

        public IList<OpenApiTagGroup>? TagGroups => null;
    }
}
