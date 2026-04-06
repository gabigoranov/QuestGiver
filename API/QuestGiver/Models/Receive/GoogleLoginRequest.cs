using System.ComponentModel.DataAnnotations;

namespace QuestGiver.Models.Receive
{
    public class GoogleLoginRequest
    {
        [Required]
        public string IdToken { get; set; }
    }
}
