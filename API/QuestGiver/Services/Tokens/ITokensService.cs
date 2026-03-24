using QuestGiver.Data.Models;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Tokens
{
    /// <summary>
    /// Handles the business logic related to token generation, validation, and management. This service is responsible for implementing the core logic for handling tokens, such as creating JWTs, validating them, and managing token lifecycles. It is used by the AuthService to generate tokens during user authentication and other token-related operations.
    /// </summary>
    public interface ITokensService
    {
        /// <summary>
        /// Generates a JWT token for the specified user. This method creates a token that can be used for authenticating the user in subsequent requests.
        /// </summary>
        /// <param name="userId">The user for whom the token is being generated.</param>
        /// <returns>A JWT token as a string.</returns>
        public string GenerateAccessToken(Guid userId);

        /// <summary>
        /// Generates a new refresh token for use in authentication workflows.
        /// </summary>
        /// <returns>A string containing the newly generated refresh token. The token is suitable for securely identifying a user
        /// session during token refresh operations.</returns>
        public string GenerateRefreshToken();

        /// <summary>
        /// Creates and saves a token entity.
        /// </summary>
        /// <param name="userId">The user id to be linked with the token.</param>
        /// <returns>The created token.</returns>
        public Task<TokenDTO> CreateTokenAsync(Guid userId);

        /// <summary>
        /// Supplies a new acess token if the refresh token is still valid.
        /// </summary>
        /// <param name="refreshToken">The refresh token supplied by the frontend.</param>
        /// <returns>The refreshed token.</returns>
        public Task<TokenDTO> RefreshTokenAsync(string refreshToken);

        /// <summary>
        /// Invalidates ( Deletes ) the supplied refresh token.
        /// </summary>
        /// <param name="refreshToken">The refresh token.</param>
        /// <returns>Nothing.</returns>
        public Task InvalidateTokenAsync(string refreshToken);
    }
}
