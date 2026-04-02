using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace QuestGiver.Controllers
{
    /// <summary>
    /// Used to manage quest votes
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class VotesController : ControllerBase
    {

        /// <summary>
        /// Loads the current vote for a quest
        /// Also returns info on the individual votes of user's from the related friend group
        /// </summary>
        /// <param name="questId">The id of the quest</param>
        /// <returns>A completion vote or a skip vote, depending on the active vote for the quest</returns>
        [HttpGet("quest/{questId}")]
        public async Task<IActionResult> GetQuestVote([FromRoute] Guid questId)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Creates a new vote
        /// </summary>
        /// <remarks>
        /// Can create either a completion vote or a skip vote depending on the model
        /// </remarks>
        /// <returns>The new vote</returns>
        [HttpPost]
        public async Task<IActionResult> CreateVote() // Will have some kind of dto when i implement it
        {
            throw new NotImplementedException(); 
        }

        /// <summary>
        /// Submits the individual vote of a user - true / false
        /// </summary>
        /// <returns></returns>
        [HttpPost("{voteId}/vote")]
        public async Task<IActionResult> SubmitIndividualVote([FromRoute] Guid voteId) // Will have some kind of dto when i implement it
        {
            throw new NotImplementedException(); 
        }
    }
}
