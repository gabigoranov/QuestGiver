using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Receive
{
    public class CreateQuestDTO
    {
        [Required]
        [StringLength(30)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        public DateTime ScheduledDate { get; set; }

        [Required]
        public int RewardPoints { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        [Required]
        [ForeignKey(nameof(FriendGroup))]
        public Guid FriendGroupId { get; set; }
    }
}
