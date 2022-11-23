using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [DisplayName("First Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? FirstName { get; set; } // ? allows for null

        [Required]
        [DisplayName("Last Name")]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and max {1} characters long.", MinimumLength = 2)]
        public string? LastName { get; set; }

        [NotMapped]
        [DisplayName("Full Name")]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        [NotMapped]
        [DataType(DataType.Upload)]
        public IFormFile? ImageFormFile { get; set; }

        public int CompanyId { get; set; }

        // Navigation Properties

        public virtual Company? Company { get; set; }

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();

    }
}
