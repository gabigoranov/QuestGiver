using QuestGiver.Models.Send;

namespace QuestGiver.Services.OAuth
{
    /// <summary>
    /// A service used for communication with the google OAuth API to authenticate users
    /// </summary>
    public interface IOAuthService
    {
        /// <summary>
        /// Handles authenticating a user via google ( sign up / sign in )
        /// </summary>
        /// <param name="idToken">The id of their oauth token</param>
        /// <returns>A valid AuthResponse</returns>
        Task<AuthResponse> LoginWithGoogleAsync(string idToken);
    }
}
