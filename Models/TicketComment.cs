using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class TicketComment
    {
        public int Id { get; set; }

        [Required]
        public string? Comment { get; set; }

        public DateTime Created { get; set; }

        public int TicketId { get; set; }

        [Required]
        public int UserId { get; set; }

        // Navigation Properties

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }
    }
}