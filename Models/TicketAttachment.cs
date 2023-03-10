using BugTrackerMVC.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    public class TicketAttachment
    {
        public int Id { get; set; }

        [DisplayName("File Description")]
        [StringLength(1000)]
        public string? Description { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Added")]
        public DateTime Created { get; set; }

        public int TicketId { get; set; } // FK

        public string? UserId { get; set; } // PK

        [DisplayName("File Name")]
        public string? FileName { get; set; }

        [NotMapped]        
		[DisplayName("Select a file")]
		[DataType(DataType.Upload)]
		[MaxFileSize(1024 * 1024)]
		[AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png", ".txt", ".doc", ".docx", ".xls", ".xlsx", ".pdf" })]
		public IFormFile? FormFile { get; set; } 

        public byte[]? FileData { get; set; }

        public string? FileType { get; set; }


        // Navigation Properties

        public virtual Ticket? Ticket { get; set; }

        public virtual BTUser? User { get; set; } // person adding attachment
    }
}
