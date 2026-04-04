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

        /// <summary>
        /// Increases the xp of the user by a specified amount and potentially levels the user up
        /// </summary>
        /// <param name="userId">The id of the user</param>
        /// <param name="xp">The xp the user should gain</param>
        /// <returns>Nothing</returns>
        Task IncreaseUserXP(Guid userId, int xp);
    }
}
