using Microsoft.AspNetCore.Authentication;
using Org.BouncyCastle.Crypto.Paddings;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [Required]
        public string? Name { get; set; }

        [Required]
        public string? Description { get; set; }

        public DateTime Created { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        public byte[]? ImageFileData { get; set; }
        
        public string? ImageFileType { get; set; }

        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        public bool Archived { get; set; }

        // Navigation Properties

        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
