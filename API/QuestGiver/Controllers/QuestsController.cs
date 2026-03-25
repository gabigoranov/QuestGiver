using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Services.Quests;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// Handles HTTP requests related to quest management.
    /// </summary>
    [Route("api/[controller]")]
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
    }
}
