using System.ComponentModel.DataAnnotations;

namespace QuestGiver.Models.Receive
{
    /// <summary>
    /// Represents the data required to authenticate a user during a login operation.
    /// </summary>
    public class LoginDTO
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        public string Password { get; set; }
    }
}
