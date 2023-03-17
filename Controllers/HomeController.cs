using BugTrackerMVC.Data;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Models.ViewModels;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Threading.Tasks;
using BugTrackerMVC.Models.ChartModels;
using System.Data;

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


        // Post: Home/GglProjectTickets
        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name!, prj.Tickets.Count() });
            }

            return Json(chartData);
        }


        // Post: Home/GglProjectPriority
        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }


        // Post: Home/AmCharts
        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {

            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Where(p => p.Archived == false).ToList();

            foreach (Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Developer))).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();

            return Json(amChartData.Data);
        }


        // Post: Home/PlotlyBarChart
        [HttpPost]
        public async Task<JsonResult> PlotlyBarChart()
        {
            PlotlyBarData plotlyData = new();
            List<PlotlyBar> barData = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            //Bar One
            PlotlyBar barOne = new()
            {
                X = projects.Select(p => p.Name).ToArray()!,
                Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
                Name = "Tickets",
                Type = "bar"
            };

            //Bar Two
            PlotlyBar barTwo = new()
            {
                X = projects.Select(p => p.Name).ToArray()!,
                Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(BTRoles.Developer))).Count).Select(c => c.Result).ToArray(),
                Name = "Developers",
                Type = "bar"
            };

            barData.Add(barOne);
            barData.Add(barTwo);

            plotlyData.Data = barData;

            return Json(plotlyData);
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