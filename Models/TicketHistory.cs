using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class TicketHistory
    {
        public int Id { get; set; } // PK

        public int TicketId { get; set; }

        public string? PropertyName { get; set; }

        [StringLength(5000)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        [DisplayName("Previous Value")]
        public string? OldValue { get; set; }

        [DisplayName("Current Value")]
        public string? NewValue { get; set; } // FK

        [Required]
        public string? UserId { get; set; } // FK

        // Navigation Properties

        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}