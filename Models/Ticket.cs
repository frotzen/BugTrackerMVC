using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class Ticket
    {
        public int Id { get; set; } // PK

        [Required]
        [StringLength(50)]
        [DisplayName("Ticket Title")]
        public string? Title { get; set; }

        [Required]
        [StringLength(2000)]
        [DisplayName("Ticket Description")]
        public string? Description { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Updated")]
        public DateTime Updated { get; set; }

        public bool Archived { get; set; }

        [DisplayName("Archived By Project")]
        public bool ArchivedByProject { get; set; }

        public int ProjectId { get; set; } // FK
        
        public int TicketTypeId { get; set; } // FK

        public int TicketStatusId { get; set; } // FK

        public int TicketPriorityId { get; set; } // FK

        public string? DeveloperUserId { get; set; } // FK

        [Required]
        public string? SubmitterUserId { get; set; } // FK

        // Navigation Properties

        public virtual Project? Project { get; set; }

        [DisplayName("Priority")]
        public virtual TicketPriority? TicketPriority { get; set; }

        [DisplayName("Type")]
        public virtual TicketType? TicketType { get; set; }

        [DisplayName("Status")]
        public virtual TicketStatus? TicketStatus { get; set; }

        [DisplayName("Ticket Owner")]
        public virtual BTUser? DeveloperUser { get; set; }
        
        [DisplayName("Submitted By")]
        public virtual BTUser? SubmitterUser { get; set; }

        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();

        public virtual ICollection<TicketAttachment> Attachments { get; set; } = new HashSet<TicketAttachment>();

        public virtual ICollection<TicketHistory> History { get; set; } = new HashSet<TicketHistory>();

        public virtual ICollection<Notification> Notifications { get; set; } = new HashSet<Notification>();
    }
}
