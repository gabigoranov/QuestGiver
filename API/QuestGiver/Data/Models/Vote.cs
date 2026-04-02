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

        /// <summary>
        /// Dynamically calculated decision
        /// Null if the majority has not voted yet
        /// If the majority has voted, then take it's decision
        /// </summary>
        [NotMapped]
        public bool? Decision
        {
            get
            {
                var totalUsers = Quest.FriendGroup.UserFriendGroups.Count();
                var requiredMajority = (int)Math.Ceiling(totalUsers / 2.0);

                var trueVotes = UserVotes.Count(v => v.Decision == true);
                var falseVotes = UserVotes.Count(v => v.Decision == false);

                if (trueVotes >= requiredMajority)
                    return true;

                if (falseVotes >= requiredMajority)
                    return false;

                return null;
            }
        }

        public virtual ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>();
    }
}
