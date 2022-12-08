using Microsoft.AspNetCore.Mvc.Rendering;
namespace BugTrackerMVC.Models.ViewModels
{
    public class TicketDetailsViewModel
    {
        public Ticket? Ticket { get; set; }
        public Project? Project { get; set; }

        public TicketComment? TicketComment { get; set; }

        public List<TicketComment>? TicketComments { get; set; }

    }
}
