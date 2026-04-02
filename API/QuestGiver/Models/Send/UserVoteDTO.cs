using QuestGiver.Data.Models;

namespace QuestGiver.Models.Send
{
    public class UserVoteDTO
    {
        public Guid UserId { get; set; }
        public Guid VoteId { get; set; }

        /// <summary>
        /// Represents the user's decision for the vote
        /// or null if they haven't decided
        /// </summary>
        public bool? Decision { get; set; }
    }
}
