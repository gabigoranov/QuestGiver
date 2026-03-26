using QuestGiver.Data.Models;

namespace QuestGiver.Data.Constants
{
    /// <summary>
    /// Expands the Quest model as the first quest of every friend group, given to the creator.
    /// </summary>
    public class InitialAddFriendsQuest : Quest
    {
        /// <summary>
        /// Constructor that assigns some default values for the initial quest.
        /// </summary>
        /// <param name="userId">Id of the friend group creator.</param>
        /// <param name="friendGroupId">Id of the friend group.</param>
        public InitialAddFriendsQuest(Guid userId, Guid friendGroupId)
        {
            Id = Guid.NewGuid();
            Title = "Add Friends";
            Description = "Add some friends to your friend group.";
            ScheduledDate = DateTime.UtcNow.Date; // Set to today's date at midnight UTC
            RewardPoints = 100;
            UserId = userId;
            FriendGroupId = friendGroupId;
        }
    }
}
