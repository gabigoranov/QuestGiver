using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Users;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// API Controller for handling user-related operations, such as registration, authentication, profile management, and other user-specific functionalities.
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        /// <summary>
        /// Constructor for UsersController, which handles DI.
        /// </summary>
        /// <param name="authService">Holds user related business logic.</param>
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // TODO: Create endpoints for: registration, login, profile management, password reset, get by id, etc

        /// <summary>
        /// Hnadles the registration of a new user, along with creating a valid JWT token for the user to use for authentication in future requests.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <returns>An AuthResponse with the UserDTO and JWT.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO model)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            AuthResponse response = await _authService.CreateUserAsync(model);
            return Ok(response);
        }

    }
}
