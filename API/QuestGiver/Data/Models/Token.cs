using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Data.Models
{
    /// <summary>
    /// Defines a token class that represents an authentication token for a user.
    /// </summary>
    public class Token
    {
        [Key]
        public int Id { get; set; }

        public DateTime ExpirationDateTime { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required]
        public string RefreshToken { get; set; }

        [NotMapped]
        public string AccessToken { get; set; }

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
