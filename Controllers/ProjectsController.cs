using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTrackerMVC.Services.Interfaces;
using BugTrackerMVC.Helper;
using BugTrackerMVC.Services;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using BugTrackerMVC.Enums;

namespace BugTrackerMVC.Controllers
{
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IFileService _fileService;
        private readonly IBTProjectService _projectService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> userManager,
                                    IFileService fileService, IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;   
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;

            List<Project> projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }

        // GET: All Projects
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AllProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId));

            return View(projects);
        }

        // GET: Archived Projects
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                           .Where(p => p.Archived == true).ToList();

            return View(projects);
        }

        // GET: Users Projects
        public async Task<IActionResult> MyProjects()
        {
            string userId = (await _userManager.GetUserAsync(User)).Id;
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            List<Project> projects = new();

            if(User.IsInRole(nameof(BTRoles.Admin)))
            {
                // call GetAllProjects from service
                projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId));

            }
            else
            {
                // Create a new service to get user projects
                // and call the service for this.user's projects
                projects = await _projectService.GetUserProjectsAsync(userId);
            }
            

            return View(projects);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");

            // ToDo: call Project Service
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,StartDate,EndDate,ProjectPriorityId,ImageFormFile")] Project project)
        {
            if (ModelState.IsValid)
            {
                //***** Method to get the companyId
                int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
                project.CompanyId = companyId;
                project.Created = PostgresDate.Format(DateTime.Now);

                if (project.ImageFormFile != null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                // format dates
                project.StartDate = PostgresDate.Format(project.StartDate);
                project.EndDate = PostgresDate.Format(project.EndDate);

                // use Project Service
                await _projectService.AddProjectAsync(project);

                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(project);
        }

        // GET: Projects/Edit/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            // ToDo: use Project Service
            var project = await _projectService.GetProjectByIdAsync(id.Value, companyId);

            if (project == null)
            {
                return NotFound();
            }

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // Update time for Postgres so a cast of Date types isn't attempted
                project.Created = PostgresDate.Format(project.Created);
                project.StartDate = PostgresDate.Format(project.StartDate);
                project.EndDate = PostgresDate.Format(project.EndDate);


                try
                {
                    _context.Update(project);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(project);
        }

        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // ToDo: use Project Service 
            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/ArchiveConfirmed/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            // ToDo: use project service
            var project = await _projectService.GetProjectByIdAsync(id, companyId);

            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project);
            }            

            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return _context.Projects.Any(e => e.Id == id);
        }
    }
}
