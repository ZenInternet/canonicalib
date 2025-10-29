using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using Test.Contract.Examples;
using Zen.CanonicaLib.DataAnnotations;

namespace Test.Contract.Controllers
{

    /// <summary>
    /// User Management
    /// </summary>
    /// <remarks>
    /// Some markdown content explaining the user management section
    /// </remarks>
    [OpenApiTag("Users")]
    [OpenApiPath("/users")]
    public interface IUsersController
    {

        /// <summary>
        /// Get Users
        /// </summary>
        /// <remarks>
        /// Gets a list of users
        /// </remarks>
        [OpenApiEndpoint("", Methods.MethodGet)]
        [Response(typeof(GetUsersResponse), StatusCodes.Status200OK, "Successfully retrieved users")]
        [ResponseExample(StatusCodes.Status200OK, typeof(UsersResponseExample))]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        public IActionResult GetUsers();


        [OpenApiEndpoint("{userId}", Methods.MethodGet)]
        [Response(typeof(User), StatusCodes.Status200OK, "Successfully retrieved user")]
        [ResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        [Response(StatusCodes.Status404NotFound, "The specified user was not found")]
        public IActionResult GetUser([FromRequestPath] Guid userId);

        [OpenApiEndpoint("", Methods.MethodPost)]
        [Response(typeof(User), StatusCodes.Status200OK, "Successfully created user")]
        [ResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
        [Response(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "Invalid request")]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        public IActionResult CreateUser([FromRequestBody] User user);

        [OpenApiEndpoint("{userId}", Methods.MethodPut)]
        [Response(typeof(User), StatusCodes.Status200OK, "Successfully updated user")]
        [ResponseExample(StatusCodes.Status200OK, typeof(UserResponseExample))]
        [Response(typeof(ProblemDetails), StatusCodes.Status400BadRequest, "Invalid request")]
        [Response(StatusCodes.Status401Unauthorized, "You must be authenticated to access this resource")]
        [Response(StatusCodes.Status403Forbidden, "You do not have permission to access this resource")]
        [Response(StatusCodes.Status404NotFound, "The specified user was not found")]
        [Response(typeof(ProblemDetails), StatusCodes.Status409Conflict, "The update could not be completed due to a conflict")]
        public IActionResult UpdateUser([FromRequestPath] Guid userId, [FromRequestBody] User user);
    }
}
