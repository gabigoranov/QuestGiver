using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Data.Models
{
    /// <summary>
    /// Represents a group of friends.
    /// </summary>
    public class FriendGroup
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [NotMapped]
        public Guid? CurrentQuestId => Quests.FirstOrDefault(x => DateTime.UtcNow.Date == x.ScheduledDate)?.Id;

        public virtual Quest? CurrentQuest => Quests.FirstOrDefault(x => DateTime.UtcNow.Date == x.ScheduledDate);

        // Many-to-many relationship with User through UserFriendGroup
        public virtual ICollection<UserFriendGroup> UserFriendGroups { get; set; } = new List<UserFriendGroup>();
        public virtual ICollection<Quest> Quests { get; set; } = new List<Quest>();
    }
}
