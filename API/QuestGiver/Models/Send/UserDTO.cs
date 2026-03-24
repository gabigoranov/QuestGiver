using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Send
{
    public class UserDTO
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

        public string? AvatarUrl { get; set; }

        // Fields for rewards system - XP, Level, etc.
        public int Level { get; set; }
        public int ExperiencePoints { get; set; }
        public int NextLevelExperience { get; set; } // XP required for next level
    }
}
