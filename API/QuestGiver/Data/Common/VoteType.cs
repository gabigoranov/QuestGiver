namespace QuestGiver.Data.Common
{
    /// <summary>
    /// Discriminator for the votes TPH
    /// </summary>
    public enum VoteType
    {
        /// <summary>
        /// used to vote whether a quest is completed
        /// </summary>
        CompletionVote = 0,

        /// <summary>
        /// used to vote whether a quest should be skipped
        /// </summary>
        SkipVote = 1 
    }
}
