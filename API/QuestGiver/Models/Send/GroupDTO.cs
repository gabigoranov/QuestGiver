using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Send
{
    public class GroupDTO
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        public DateTime DateCreated { get; set; }

        public int MembersCount { get; set; }

        // If the group has a quest for today, include its status; otherwise, this will be null
        public QuestStatusType? CurrentQuestStatus { get; set; }
    }
}
