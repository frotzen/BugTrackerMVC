using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class EmailData
    {
        public int Id { get; set; }

        [Required]
        [DataType(DataType.EmailAddress)]
        public string? EmailAddress { get; set; }

        [Required]
        public string? EmailSubject { get; set; }

        [Required]
        public string? EmailBody { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? GroupName { get; set; }
    }
}
