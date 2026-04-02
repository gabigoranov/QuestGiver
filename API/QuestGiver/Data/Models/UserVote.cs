namespace QuestGiver.Data.Models
{
    /// <summary>
    /// A kind of many-to-many join table used to register the 
    /// individual vote of a user.
    /// </summary>
    public class UserVote
    {
        public UserVote(Guid userId, Guid voteId)
        {
            UserId = userId;
            VoteId = voteId;
        }

        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid VoteId { get; set; }
        public virtual Vote Vote { get; set; }

        /// <summary>
        /// Represents the user's decision for the vote
        /// or null if they haven't decided
        /// </summary>
        public bool? Decision { get; set; }
    }
}
