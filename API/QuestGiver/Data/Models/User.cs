using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Data.Models
{
    /// <summary>
    /// The User class - represents a user in the system.
    /// </summary>
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [StringLength(30)]
        public string Username { get; set; }

        [Required]
        public DateTime BirthDate { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string PasswordHash { get; set; }

        public string? AvatarUrl { get; set; }

        // Navigation properties
        public virtual UserSettings Settings { get; set; }

        // Many-to-many relationship with FriendGroup through UserFriendGroup
        public virtual ICollection<UserFriendGroup> UserFriendGroups { get; set; } = new List<UserFriendGroup>();
        public virtual ICollection<Quest> Quests { get; set; } = new List<Quest>();
        public virtual ICollection<Token> Tokens { get; set; } = new List<Token>(); // Use a One-to-Many relationship for tokens to allow multiple active sessions on different devices
        public virtual ICollection<UserVote> UserVotes { get; set; } = new List<UserVote>(); // Use a One-to-Many relationship for tokens to allow multiple active sessions on different devices

        [NotMapped]
        public virtual IEnumerable<Quest> CurrentQuests => Quests.Where(x => DateTime.UtcNow.Date == x.ScheduledDate); // Runtime calculated property to get the most recent quest

        // Fields for rewards system - XP, Level, etc.
        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; } = 0;
        public int NextLevelExperience { get; set; } = 100; // XP required for next level

    }
}
