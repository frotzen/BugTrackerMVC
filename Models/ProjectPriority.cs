using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BugTrackerMVC.Models
{
    public class ProjectPriority
    {
        public int Id { get; set; }

        [Required]
        [DisplayName("Project Priority Name")]
        public string? Name { get; set; }
    }
}
