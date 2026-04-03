using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Extensions;
using QuestGiver.Models.Send;
using QuestGiver.Services.Quests;
using System.Text.RegularExpressions;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// Handles HTTP requests related to quest management.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class QuestsController : ControllerBase
    {
        private readonly IQuestsService _questsService;

        /// <summary>
        /// Handles DI.
        /// </summary>
        /// <param name="questsService">The quests service.</param>
        public QuestsController(IQuestsService questsService)
        {
            _questsService = questsService;
        }

        // TODO: Implement endpoints for retrivient the current friend group quest and completing a quest

        /// <summary>
        /// Retrieves the current quest for a friend group and if needed updates the quest queee.
        /// </summary>
        /// <param name="groupId">The id of the group</param>
        /// <returns>The QuestDTO.</returns>
        [HttpGet("group/{groupId}")]
        public async Task<IActionResult> GetCurrentQuest([FromRoute] Guid groupId)
        {
            // Load userId from JWT token
            Guid userId = User.GetUserId();

            QuestDTO quest = await _questsService.GetCurrentQuestForGroupAsync(groupId, userId);

            return Ok(quest);
        }

        /// <summary>
        /// Loads the authenticated user's own quest history ( all of their quests )
        /// </summary>
        /// <returns>A list of quests</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetUserQuestHistory()
        {
            // Load userId from JWT token
            Guid userId = User.GetUserId();

            List<QuestDTO> quests = await _questsService.GetAllUserQuests(userId);

            return Ok(quests);
        }

        /// <summary>
        /// Completes a quest
        /// </summary>
        /// <param name="questId">The quest to be completed</param>
        /// <returns>Nothing</returns>
        [HttpPost("complete/{questId}")]
        public async Task<IActionResult> Complete([FromRoute] Guid questId)
        {
            // Load userId from JWT token
            Guid userId = User.GetUserId();

            QuestDTO updated = await _questsService.CompleteQuestAsync(questId, userId);

            return Ok(updated);
        }
    }
}
