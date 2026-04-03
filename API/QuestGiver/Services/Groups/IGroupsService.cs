using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Groups
{
    /// <summary>
    /// Handles business logic related to friend groups.
    /// </summary>
    public interface IGroupsService
    {
        /// <summary>
        /// Creates a new friend group with the provided details and returns the created group information.
        /// </summary>
        /// <param name="model">The input model.</param>
        /// <param name="userId">The Id of the friend group creator.</param>
        /// <returns>The created group DTO.</returns>
        public Task<GroupDTO> CreateGroupAsync(CreateGroupDTO model, Guid userId);

        /// <summary>
        /// Creates a new entry in the UserFriendsGroup join table.
        /// </summary>
        /// <param name="groupId">The id of the friend group.</param>
        /// <param name="userId">The id of the new friend to be added.</param>
        /// <returns>Nothing.</returns>
        public Task AddUserToGroupAsync(Guid groupId, Guid userId);

        /// <summary>
        /// Removes a user from a friend group by deleting the corresponding entry in the UserFriendsGroup join table.
        /// </summary>
        /// <param name="groupId">The friend group id.</param>
        /// <param name="userId">The user id.</param>
        /// <returns></returns>
        Task RemoveUserFromGroupAsync(Guid groupId, Guid userId);

        /// <summary>
        /// Retrieves all friend groups that a user belongs to.
        /// </summary>
        /// <param name="userId">The id of the authenticated user.</param>
        /// <returns>A list of friend groups.</returns>
        Task<List<GroupDTO>> GetGroupsForUserAsync(Guid userId);

        /// <summary>
        /// If the userId belongs to the group, returns the group
        /// </summary>
        /// <param name="groupId">The group id</param>
        /// <param name="userId">The user id</param>
        /// <returns>A group dto</returns>
        Task<GroupDTO> GetGroupByIdAsync(Guid groupId, Guid userId);

        /// <summary>
        /// Returns all the users who belong to a group
        /// </summary>
        /// <remarks>
        /// To access them, the user needs to also belong to the group
        /// </remarks>
        /// <param name="groupId">The id of the group</param>
        /// <param name="userId">The id of the user</param>
        /// <returns>A collection of user dtos</returns>
        Task<List<UserDTO>> GetGroupMembersAsync(Guid groupId, Guid userId);
    }
}
