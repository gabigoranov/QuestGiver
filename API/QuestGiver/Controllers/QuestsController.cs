using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Extensions;
using QuestGiver.Models.Send;
using QuestGiver.Services.Quests;

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
    }
}
