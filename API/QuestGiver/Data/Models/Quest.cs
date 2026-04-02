using QuestGiver.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Data.Models
{
    public class Quest
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Title { get; set; }

        [Required]
        [StringLength(400)]
        public string Description { get; set; }

        // ALWAYS SET TO MIDNIGHT UTC - this is the date the quest is scheduled for, not when it was created or completed
        public DateTime ScheduledDate { get; set; } 
        public DateTime? DateCompleted { get; set; }

        [NotMapped]
        public bool IsCompleted => DateCompleted.HasValue; // Runtime calculated completion flag

        [Required]
        public int RewardPoints { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }

        [Required]
        [ForeignKey(nameof(FriendGroup))]
        public Guid FriendGroupId { get; set; }

        public virtual FriendGroup FriendGroup { get; set; }

        public QuestStatusType Status { get; set; } = QuestStatusType.New; // Default to New when created

        /// <summary>
        /// Navigational property for the one to many relationship
        /// Can have a completion vote, can have a skip vote
        /// </summary>
        public ICollection<Vote> Votes { get; set; } = new List<Vote>();
    }
}
