using Zen.CanonicaLib.DataAnnotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Zen.Contract.Examples;

namespace Zen.Contract.Controllers
{

    /// <summary>
    /// User Management
    /// </summary>
    /// <remarks>
    /// Some markdown content explaining the user management section
    /// </remarks>
    [Path("/users")]
    public interface IUsersController
    {

        /// <summary>
        /// Get Users
        /// </summary>
        /// <remarks>
        /// Gets a list of users
        /// </remarks>
        [Endpoint("", Methods.MethodGet)]
        [Response(StatusCodes.Status200OK, "Successfully retrieved users", typeof(GetUsersResponse))]
        [ResponseExample(StatusCodes.Status200OK, typeof(UsersResponseExample))]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        public IActionResult GetUsers();


        [Endpoint("{userId}", Methods.MethodGet)]
        [Response(StatusCodes.Status200OK, "Successfully retrieved user", typeof(User))]
        [ResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        [Response(StatusCodes.Status404NotFound, "The specified user was not found")]
        public IActionResult GetUser([FromRequestPath] Guid userId);

        [Endpoint("", Methods.MethodPost)]
        [Response(StatusCodes.Status200OK, "Successfully created user",typeof(User))]
        [ResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
        [Response(StatusCodes.Status400BadRequest, "Invalid request", typeof(ProblemDetails))]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        public IActionResult CreateUser([FromRequestBody] User user);

        [Endpoint("{userId}", Methods.MethodPut)]
        [Response(StatusCodes.Status200OK, "Successfully updated user", typeof(User))]
        [ResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
        [Response(StatusCodes.Status400BadRequest, "Invalid request", typeof(ProblemDetails))]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        [Response(StatusCodes.Status404NotFound, "The specified user was not found")]
        [Response(StatusCodes.Status409Conflict, "The update could not be completed due to a conflict", typeof(ProblemDetails))]
        public IActionResult UpdateUser([FromRequestPath] Guid userId, [FromRequestBody] User user);
    }
}
