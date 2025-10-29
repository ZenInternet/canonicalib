
using Zen.CanonicaLib.DataAnnotations;
using System;
using System.Collections.Generic;

namespace Test.Contract.Examples
{
    public class UsersResponseExample : IExample<GetUsersResponse>
    {
        public string Name => "Get a list of users";

        public GetUsersResponse Example => new GetUsersResponse()
        {
            Users = new List<User>()
            {
                new User() { Id = Guid.NewGuid(), FirstName = "Ben", LastName = "Wolstencroft" },
                new User() { Id = Guid.NewGuid(), FirstName = "Julian", LastName = "Monono" }
            },
            Count = 2
        };
    }
}
