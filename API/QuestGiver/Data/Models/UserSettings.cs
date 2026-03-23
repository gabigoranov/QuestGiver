using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Data.Models
{
    /// <summary>
    /// Represents a set of configurable settings for a user.
    /// </summary>
    public class UserSettings
    {
        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
        public bool ReceiveNotifications { get; set; } = true;

        // Additional settings can be added here as needed
    }
}
