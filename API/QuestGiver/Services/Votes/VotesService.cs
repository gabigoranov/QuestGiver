using QuestGiver.Models.Receive;
using QuestGiver.Models.Send;

namespace QuestGiver.Services.Votes
{
    /// <inheritdoc />
    public class VotesService : IVotesService
    {
        /// <inheritdoc />
        public Task<VoteDTO> CreateVoteAsync(CreateVoteDTO vote, Guid userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task<VoteDTO> GetActiveQuestVoteAsync(Guid questId, Guid userId)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Task SubmitIndividualVoteAsync(Guid voteId, Guid userId, bool decision)
        {
            throw new NotImplementedException();
        }
    }
}
