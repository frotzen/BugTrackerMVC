using Microsoft.EntityFrameworkCore.Query;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class Invite
    {
        public int Id { get; set; } // PK

        [DataType(DataType.DateTime)]
        [DisplayName("Date Invited")]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.DateTime)]
        [DisplayName("Date Joined")]
        public DateTime JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        public int CompanyId { get; set; } // FK

        public int ProjectId { get; set; } // FK

        [Required]
        public string? InviterId { get; set; } // FK

        public string? InviteeId { get; set; } // FK

        [Required]
        public string? InviteeEmail { get; set; }

        [Required]
        [DisplayName("First Name")]
        public string? InviteeFirstName { get; set; }

        [Required]
        [DisplayName("Last Name")]
        public string? InviteeLastName { get; set; }

        public string? Message { get; set; }

        public bool IsValid { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Inviter { get; set; }

        public virtual BTUser? Invitee { get; set; }


    }
}
