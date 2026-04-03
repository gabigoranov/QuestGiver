using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Models.Send;
using QuestGiver.Services.Users;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// Provides endpoints for getting by id, updating, etc. 
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;

        /// <summary>
        /// Initializes a new instance of the UsersController class with the specified user service.
        /// </summary>
        /// <param name="usersService">The service used to manage and retrieve user data. Cannot be null.</param>
        public UsersController(IUsersService usersService)
        {
            _usersService = usersService;
        }


        // Not very secure, but used for development purposes
        /// <summary>
        /// Endpoint for getting a user by their id
        /// </summary>
        /// <param name="userId">The id used to find the user</param>
        /// <returns>A user dto</returns>
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserById([FromRoute] Guid userId)
        {
            UserDTO res = await _usersService.GetByIdAsync(userId);
            return Ok(res);
        }
    }
}
