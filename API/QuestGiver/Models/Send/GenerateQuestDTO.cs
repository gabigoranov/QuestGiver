namespace QuestGiver.Models.Send
{
    /// <summary>
    /// A model that is sent to the LLM to generate quests for a friend group
    /// </summary>
    public class GenerateQuestDTO
    {
        public Guid UserId { get; set; }
        public Guid FriendGroupId { get; set; }
        public string Username { get; set; }
        public string UserDescription { get; set; }
        public DateTime UserBirthDate { get; set; }
    }
}
