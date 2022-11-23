using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class TicketComment
    {
        public int Id { get; set; } // PK

        [Required]
        [DisplayName("Comment")]
        [StringLength(2000)]
        public string? Comment { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        public int TicketId { get; set; } // FK

        [Required]
        public string? UserId { get; set; } // FK

        // Navigation Properties
        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; }
    }
}