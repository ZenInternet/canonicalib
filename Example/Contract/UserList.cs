using System.Collections.Generic;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract
{
    [OpenApiTag("Models")]
    public class UserList : List<User>
    {
    }
}
