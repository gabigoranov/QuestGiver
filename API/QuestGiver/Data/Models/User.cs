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
        public virtual Token Token { get; set; }
        public virtual UserSettings Settings { get; set; }

        // Many-to-many relationship with FriendGroup through UserFriendGroup
        public virtual ICollection<UserFriendGroup> UserFriendGroups { get; set; } = new List<UserFriendGroup>();
        public virtual ICollection<Quest> Quests { get; set; } = new List<Quest>();

        [NotMapped]
        public virtual Quest CurrentQuest => Quests.Last(); // Runtime calculated property to get the most recent quest

        // Fields for rewards system - XP, Level, etc.
        public int Level { get; set; } = 1;
        public int ExperiencePoints { get; set; } = 0;
        public int NextLevelExperience { get; set; } = 100; // XP required for next level

    }
}
