using QuestGiver.Models.Send;

namespace QuestGiver.Services.Users
{
    /// <summary>
    /// Handles logic for user info outside of auth
    /// </summary>
    public interface IUsersService
    {
        /// <summary>
        /// Loads a user by ther id
        /// </summary>
        /// <param name="userId">The id used to load the user from the db</param>
        /// <returns>A user dto</returns>
        Task<UserDTO> GetByIdAsync(Guid userId);
    }
}
