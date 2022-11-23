using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Ticket Status")]
        public string? Name { get; set; }
    }
}
