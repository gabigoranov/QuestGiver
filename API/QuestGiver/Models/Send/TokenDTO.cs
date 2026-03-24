using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Send
{
    public class TokenDTO
    {
        [Key]
        public int Id { get; set; }

        public DateTime ExpirationDateTime { get; set; } = DateTime.UtcNow.AddDays(7);

        [Required]
        public string RefreshToken { get; set; }

        [NotMapped]
        public string AccessToken { get; set; }
    }
}
