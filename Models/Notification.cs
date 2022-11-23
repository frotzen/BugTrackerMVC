using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class Notification
    {
        public int Id { get; set; } // PK

        public int? ProjectId { get; set; }

        public int? TicketId { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [Required]
        public string? SenderId { get; set; } // FK

        [Required]
        public string? RecipientId { get; set; } // FK

        public int NotificationTypeId { get; set; } // FK

        public bool HasBeenViewed { get; set; }

        // Navigation Properties

        public virtual NotificationType? NotificationType { get; set; }

        public virtual Ticket? Ticket { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Sender { get; set; }

        public virtual BTUser? Recipient { get; set; }
    }
}
