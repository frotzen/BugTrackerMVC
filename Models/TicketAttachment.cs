using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        public string? Description { get; set; }

        public DateTime Created { get; set; }

        public int TicketId { get; set; }

        public string? UserId { get; set; }

        [NotMapped]
        public IFormFile? FormFile { get; set; } 

        public byte[]? FileData { get; set; }

        public string? FileType { get; set; }


        // Navigation Properties

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; } // person adding attachment
    }
}
