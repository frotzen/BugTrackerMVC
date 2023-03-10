using BugTrackerMVC.Data;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models;
using BugTrackerMVC.Models.ViewModels;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BugTrackerMVC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyService _companyService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;

        public HomeController(ILogger<HomeController> logger,
                                        ApplicationDbContext context,
                                        UserManager<BTUser> userManager,
                                        IBTCompanyService companyService,
                                        IBTProjectService projectService,
                                        IBTTicketService ticketService)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _companyService = companyService;
            _projectService = projectService;
            _ticketService = ticketService;
        }

        // Get: Home/Index
        public IActionResult Index()
        {
            return View();
        }

        // Get: Home/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            DashboardViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();
            model.Company = await _companyService.GetCompanyInfoAsync(companyId);
            model.Projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                .Where(p => p.Archived == false)
                .ToList();
            model.Tickets = model.Projects.SelectMany(p => p.Tickets)
                .Where(t => t.Archived == false)
                .ToList();
            model.Members = model.Company.Members.ToList();

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}