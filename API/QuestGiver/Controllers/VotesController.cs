using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using QuestGiver.Extensions;
using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;
using QuestGiver.Services.Votes;

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
        private readonly IVotesService _votesService;

        /// <summary>
        /// Initializes a new instance of the VotesController class with the specified votes service.
        /// </summary>
        /// <param name="votesService">The service used to manage and process vote-related operations. Cannot be null.</param>
        public VotesController(IVotesService votesService)
        {
            _votesService = votesService;
        }

        /// <summary>
        /// Loads the current vote for a quest
        /// Also returns info on the individual votes of user's from the related friend group
        /// </summary>
        /// <param name="questId">The id of the quest</param>
        /// <returns>A completion vote or a skip vote, depending on the active vote for the quest</returns>
        [HttpGet("quest/{questId}")]
        public async Task<IActionResult> GetQuestVote([FromRoute] Guid questId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load userId from JWT token
            Guid userId = User.GetUserId();

            VoteDTO activeVote = await _votesService.GetLatestQuestVoteAsync(questId, userId);

            return Ok(activeVote);
        }

        /// <summary>
        /// Creates a new vote
        /// </summary>
        /// <remarks>
        /// Can create either a completion vote or a skip vote depending on the model
        /// </remarks>
        /// <returns>The new vote</returns>
        [HttpPost]
        public async Task<IActionResult> CreateVote([FromBody] CreateVoteDTO model) 
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load userId from JWT token
            Guid userId = User.GetUserId();

            VoteDTO res = await _votesService.CreateVoteAsync(model, userId);

            return Ok(res);
        }

        /// <summary>
        /// Submits the individual vote of a user - true / false
        /// </summary>
        /// <returns></returns>
        [HttpPost("{voteId}/vote")]
        public async Task<IActionResult> SubmitIndividualVote([FromRoute] Guid voteId, [FromBody] SubmitVoteRequest model)
        { 
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Load userId from JWT token
            Guid userId = User.GetUserId();

            await _votesService.SubmitIndividualVoteAsync(voteId, userId, model.Decision);

            return Ok();
        }
    }
}
