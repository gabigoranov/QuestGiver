namespace QuestGiver.Models.Receive
{
    /// <summary>
    /// What we receive from the llm when a quest is generated.
    /// </summary>
    public class GeneratedQuestDTO
    {
        public Guid UserId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public int RewardPoints { get; set; }
    }
}
