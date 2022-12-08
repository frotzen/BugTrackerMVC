using Microsoft.AspNetCore.Authentication;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }

        [Required]
        [DisplayName("Project Name")]
        [StringLength(240, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }

        [Required]
        [DisplayName("Project Description")]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }


        [DataType(DataType.DateTime)]
        [DisplayName("Project Creation Date")]
        public DateTime Created { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Project Start Date")]
        public DateTime StartDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Project End Date")]
        public DateTime EndDate { get; set; }

        public int ProjectPriorityId { get; set; }

        [DisplayName("Project Image")]
        public byte[]? ImageFileData { get; set; }

        [DisplayName("File Extension")]
        public string? ImageFileType { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        public bool Archived { get; set; }

        // Navigation Properties

        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
