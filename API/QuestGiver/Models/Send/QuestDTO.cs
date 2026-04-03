using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Send
{
    public class QuestDTO
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
        public DateTime? DateCompleted { get; set; }
        public DateTime ScheduledDate { get; set; }

        [NotMapped]
        public bool IsCompleted => DateCompleted.HasValue; // Runtime calculated completion flag

        public QuestStatusType Status { get; set; }

        public bool HasActiveVote { get; set; } = false;

        [Required]
        public int RewardPoints { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }
    }
}
