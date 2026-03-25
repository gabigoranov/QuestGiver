using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Extensions;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Groups;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// Endpoints for managing friend groups, including creating groups, adding/removing friends, and retrieving group information.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupsService _groupsService;

        /// <summary>
        /// Constructor for GroupsController, injecting the IGroupsService to handle business logic related to friend groups.
        /// </summary>
        /// <param name="groupsService">The friend groups service.</param>
        public GroupsController(IGroupsService groupsService)
        {
            _groupsService = groupsService;
        }

        /// <summary>
        /// Retrieves all friend groups that the authenticated user is a member of.
        /// </summary>
        /// <returns>A list of friend groups.</returns>
        [HttpGet]
        public async Task<IActionResult> GetUserGroups()
        {
            // Load userId from JWT token
            Guid userId = User.GetUserId();

            GroupDTO groups = await _groupsService.GetGroupsForUserAsync(userId);
            return Ok(groups);
        }

        /// <summary>
        /// Handles creating a new friend group.
        /// </summary>
        /// <param name="model">The expected input model for the friend group.</param>
        /// <returns>The created group dto.</returns>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateGroupDTO model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load userId from JWT token
            Guid userId = User.GetUserId();

            var result = await _groupsService.CreateGroupAsync(model, userId);
            return Ok(result);
        }

        /// <summary>
        /// Adds a user to an existing friend group, creating a new entry in the UserFriendsGroup join table.
        /// </summary>
        /// <param name="groupId">The id of the group.</param>
        /// <returns>Nothing.</returns>
        [HttpPost("join")]
        public async Task<IActionResult> JoinGroup([FromBody] Guid groupId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load userId from JWT token
            Guid userId = User.GetUserId(); // We rely on the id from the claim for security

            await _groupsService.AddUserToGroupAsync(groupId, userId);

            return Ok();
        }

        /// <summary>
        /// Removes a user from a friend group by deleting the corresponding entry in the UserFriendsGroup join table.
        /// </summary>
        /// <param name="groupId">The id of the group.</param>
        /// <returns>Nothing.</returns>
        [HttpPost]
        public async Task<IActionResult> LeaveGroup([FromBody] Guid groupId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load userId from JWT token
            Guid userId = User.GetUserId(); // We rely on the id from the claim for security

            await _groupsService.RemoveUserFromGroupAsync(groupId, userId);

            return Ok();
        }
    }
}
