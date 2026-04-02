using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Votes
{
    /// <summary>
    /// Contains logic for managing votes, user votes, etc.
    /// </summary>
    public interface IVotesService
    {
        /// <summary>
        /// Retrieves the active ( most recent undecided ) vote for a quest
        /// </summary>
        /// <param name="questId">The id of the quest</param>
        /// <param name="userId">The id of the user, used for auth</param>
        /// <returns>A completion vote or skip vote</returns>
        Task<VoteDTO> GetActiveQuestVoteAsync(Guid questId, Guid userId);

        /// <summary>
        /// Creates a new vote for a specified quest
        /// </summary>
        /// <param name="vote">The vote model, contains the vote type discriminator</param>
        /// <param name="userId">The user id</param>
        /// <remarks>
        /// Only the user chosen for a certain quest can try to create a vote ( skip or complete )
        /// </remarks>
        /// <returns>The new vote</returns>
        Task<VoteDTO> CreateVoteAsync(CreateVoteDTO vote, Guid userId);

        /// <summary>
        /// Updates the related UserVote with the specified decision, if the user has rights
        /// </summary>
        /// <param name="voteId">The related vote entity</param>
        /// <param name="userId">The user id, part of the group</param>
        /// <param name="decision">The user's decision ( true / false )</param>
        /// <returns>Nothing</returns>
        Task SubmitIndividualVoteAsync(Guid voteId, Guid userId, bool decision);
    }
}
