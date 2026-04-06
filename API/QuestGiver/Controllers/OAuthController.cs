using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.OAuth;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// Handles different OAuth logins
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class OAuthController : ControllerBase
    {
        private readonly IOAuthService _oauthService;

        public OAuthController(IOAuthService oauthService)
        {
            _oauthService = oauthService;
        }

        /// <summary>
        /// Handles OAuth login with google account
        /// </summary>
        /// <param name="request">The request containing the OAuth token id</param>
        /// <returns>A valid access token for the user</returns>
        [HttpPost("google")]
        public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            AuthResponse res = await _oauthService.LoginWithGoogleAsync(request.IdToken);
            return Ok(res);
        }
    }
}
