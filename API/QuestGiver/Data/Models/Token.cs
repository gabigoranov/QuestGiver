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

        public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required]
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual User User { get; set; }
    }
}
