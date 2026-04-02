using System.ComponentModel.DataAnnotations;

namespace QuestGiver.Models.Receive
{
    public class SubmitVoteRequest
    {
        [Required]
        public bool Decision { get; set; }
    }
}
