
using CanonicaLib.DataAnnotations;
using System;

namespace Zen.Contract.Examples
{
    public class UserResponseExample : IExample<User>
    {
        public string Name => "Get a list of users";

        public User Example => new User()
        {
            Id = Guid.NewGuid(),
            FirstName = "Ben",
            LastName = "Wolstencroft"
        };
    }
}
