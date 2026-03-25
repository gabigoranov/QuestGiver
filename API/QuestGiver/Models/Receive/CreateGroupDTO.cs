using QuestGiver.Data.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QuestGiver.Models.Receive
{
    public class CreateGroupDTO
    {
        [Required]
        [StringLength(30)]
        public string Title { get; set; }

        [Required]
        [StringLength(200)]
        public string Description { get; set; }

    }
}
