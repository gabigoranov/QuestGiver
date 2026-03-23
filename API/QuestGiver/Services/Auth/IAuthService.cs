using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Users
{
    /// <summary>
    /// Handles user-related business logic, such as registration, authentication, profile management, and other user-specific functionalities. This service is responsible for implementing the core logic for user operations and is used by the AuthController to process incoming requests related to users.
    /// </summary>
    public interface IAuthService
    {
        /// <summary>
        /// Creates a new user by processing the model, hashing password, and saving the user to the database. Also, orchestrates token generation.
        /// </summary>
        /// <param name="model">The input user model.</param>
        /// <returns>A user dto ready to be sent to the frontend along with the token.</returns>
        public Task<AuthResponse> CreateUserAsync(CreateUserDTO model);
    }
}
