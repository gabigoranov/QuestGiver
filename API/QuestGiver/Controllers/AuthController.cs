using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Tokens;
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
        private readonly ITokensService _tokensService;

        /// <summary>
        /// Constructor for UsersController, which handles DI.
        /// </summary>
        /// <param name="authService">Holds user related business logic.</param>
        /// <param name="tokensService">Handles Auth Token related operations.</param>
        public AuthController(IAuthService authService, ITokensService tokensService)
        {
            _authService = authService;
            _tokensService = tokensService;
        }

        /// <summary>
        /// Hnadles the registration of a new user, along with creating a valid JWT token for the user to use for authentication in future requests.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <returns>An AuthResponse with the UserDTO and JWT.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AuthResponse response = await _authService.CreateUserAsync(model);
            return Ok(response);
        }

        /// <summary>
        /// Handles the login of a user, along with creating a JWT token for future requests.
        /// </summary>
        /// <param name="model">The login info.</param>
        /// <returns>An AuthResponse with the UserDTO and JWT.</returns>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AuthResponse response = await _authService.VerifyLoginAsync(model);
            return Ok(response);
        }

        /// <summary>
        /// Supplies a new access and refresh token if the supplied one is not expired.
        /// </summary>
        /// <param name="request">The refresh token supplied by the frontend.</param>
        /// <returns>A new TokenDTO.</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AuthResponse refreshed = await _authService.RefreshLogin(request.RefreshToken);
            return Ok(refreshed);
        }

        /// <summary>
        /// Invalidates ( Deletes ) a token or does nothing if the refreshToken is invalid.
        /// </summary>
        /// <param name="request">The refresh token.</param>
        /// <returns>Nothing.</returns>
        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> InvalidateRefreshToken([FromBody] RefreshTokenRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            await _tokensService.InvalidateTokenAsync(request.RefreshToken);
            return Ok();
        }
    }
}
