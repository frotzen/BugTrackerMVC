﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugTrackerMVC.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string? Name { get; set; }

        public string? Description { get; set; }

        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }
        
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }

        // Navigation Properties

        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>(); // *

        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();

        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
