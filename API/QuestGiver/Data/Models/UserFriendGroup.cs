namespace QuestGiver.Data.Models
{
    /// <summary>
    /// A helper class to represent the many-to-many relationship between Users and FriendGroups.
    /// </summary>
    public class UserFriendGroup
    {
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public Guid FriendGroupId { get; set; }
        public virtual FriendGroup FriendGroup { get; set; }
    }
}
