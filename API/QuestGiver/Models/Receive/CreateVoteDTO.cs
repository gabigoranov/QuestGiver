using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Receive
{
    public class CreateVoteDTO
    {
        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public VoteType Discriminator { get; set; }

        [Required]
        [ForeignKey(nameof(Quest))]
        public Guid QuestId { get; set; }
    }
}
