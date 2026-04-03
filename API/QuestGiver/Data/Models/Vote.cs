using QuestGiver.Data.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace QuestGiver.Data.Models
{
    /// <summary>
    /// Used as the base class for votes
    /// </summary>
    public abstract class Vote
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        public VoteType Discriminator { get; set; }

        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Required]
        [ForeignKey(nameof(Quest))]
        public Guid QuestId { get; set; }

        /// <summary>
        /// Navigational property 
        /// </summary>
        public virtual Quest Quest { get; set; }

        public bool? Decision { get; set; }

        /// <summary>
        /// Recalculates the vote result based on current user votes.
        /// Should be called whenever a UserVote is added/updated.
        /// </summary>
        public void RecalculateDecision(int totalUsers)
        {
            var requiredMajority = (int)Math.Ceiling(totalUsers / 2.0);

            var yesVotes = UserVotes.Count(v => v.Decision == true);
            var noVotes = UserVotes.Count(v => v.Decision == false);

            if (yesVotes >= requiredMajority)
            {
                Decision = true;
                return;
            }

            if (noVotes >= requiredMajority)
            {
                Decision = false;
                return;
            }

            Decision = null;
        }

        public virtual ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();
    }
}
