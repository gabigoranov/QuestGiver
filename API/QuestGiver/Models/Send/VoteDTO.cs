using QuestGiver.Data.Common;
using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Send
{
    public class VoteDTO
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public VoteType Discriminator { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey(nameof(Quest))]
        public Guid QuestId { get; set; }

        [NotMapped]
        public bool? Decision { get; set; }

        public virtual ICollection<UserVoteDTO> UserVotes { get; set; } = new List<UserVoteDTO>();
    }
}
