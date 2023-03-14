using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugTrackerMVC.Services.Interfaces;
using BugTrackerMVC.Helper;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models.ViewModels;

namespace BugTrackerMVC.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;

        public ProjectsController(UserManager<BTUser> userManager,
                                    IBTFileService fileService, IBTProjectService projectService,
                                    IBTTicketService ticketService, IBTRolesService rolesService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _ticketService = ticketService;
            _rolesService = rolesService;
        }

        // GET: All Projects
        public async Task<IActionResult> AllProjects()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId));

            return View(projects);
        }


        // GET: Archived Projects
        public async Task<IActionResult> ArchivedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = await _projectService.GetArchivedProjectsByCompanyIdAsync(companyId);

            return View(projects);
        }


        // GET: Users Projects
        public async Task<IActionResult> MyProjects()
        {
            string userId = (await _userManager.GetUserAsync(User)).Id;
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = new();
            
            // Call service to get user projects
            projects = await _projectService.GetUserProjectsAsync(userId);
            
            return View(projects);
        }


        // Get: Unassigned Projects
        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> UnassignedProjects()
        {
            string userId = (await _userManager.GetUserAsync(User)).Id;
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = (await _projectService.GetAllUnassignedProjectsByCompanyIdAsync(companyId));
                       
            return View(projects);
        }


        // Get: Assign Project Manager
        [HttpGet]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        public async Task<IActionResult> AssignPM(int? id)
        {
            if(id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            string roleName = nameof(BTRoles.ProjectManager); // set PM as role
            List<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(roleName, companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id.Value);

            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id.Value),
                PMId = currentPM?.Id,
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id)
            };

            return View(viewModel);
        }


        [HttpPost]
        [Authorize(Roles = nameof(BTRoles.Admin))]
        [ValidateAntiForgeryToken]
        // POST: Assign Project Manager
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if(viewModel.Project?.Id != null)
            {
                if(!string.IsNullOrEmpty(viewModel.PMId))
                {
                    BTUser newPM = await _userManager.FindByIdAsync(viewModel.PMId);
                    await _projectService.AddProjectManagerAsync(newPM, viewModel.Project.Id);
                }
                else
                {
                    await _projectService.RemoveProjectManagerAsync(viewModel.Project.Id);
                }
            }

            return RedirectToAction(nameof(Details), new { id = viewModel.Project?.Id });
        }


        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        // GET: Projects/AssignMembers
        public async Task<IActionResult> AssignMembers(int id)
        {
            ProjectMembersViewModel model = new();
            int companyId = User.Identity!.GetCompanyId();
            model.Project = await _projectService.GetProjectByIdAsync(id);

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> companyMembers = developers.Concat(submitters).ToList();

            List<string> projectMembers = model.Project.Members.Select(m => m.Id).ToList();
            model.Users = new MultiSelectList(companyMembers, "Id", "FullName", projectMembers);
            
            return View(model);
        }


        [HttpPost]        
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        // POST: Projects/AssignMembers
        public async Task<IActionResult> AssignMembers(ProjectMembersViewModel model)
        {
            if(model.SelectedUsers != null)
            {
                List<BTUser> projectMembers = await _projectService.GetAllProjectMembersExceptPMAsync(model.Project.Id);
                // Remove current members
                foreach(BTUser member in projectMembers)
                {
                    await _projectService.RemoveMemberFromProjectAsync(member, model.Project.Id);
                }

                // Add selected members returned from View
                foreach(string member in model.SelectedUsers)
                {
                    BTUser thisMember = await _userManager.FindByIdAsync(member);
                    await _projectService.AddMemberToProjectAsync(thisMember, model.Project.Id);
                }

                // Return to project details
                return RedirectToAction("Details", "Projects", new { id = model.Project.Id});
            }

            return RedirectToAction(nameof(AssignMembers), new {id = model.Project.Id});
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            Project project = await _projectService.GetProjectByIdAsync(id.Value);
            ViewData["Tickets"] = await _ticketService.GetAllTicketsByProjectIdAsync(project.Id);
            ViewData["ProjectManager"] = await _projectService.GetProjectManagerAsync(project.Id);

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
            List<BTUser> projectManagers = await _rolesService
                    .GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), User.Identity!.GetCompanyId());

            AssignPMViewModel viewModel = new()
            {
                Project = new(),
                PMList = new SelectList(projectManagers, "Id", "FullName"),
                PMId = null
            };

            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name");
            // call Project Service for view data
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            
            return View(viewModel);
        }


        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AssignPMViewModel viewModel)
        {
            Project project = viewModel.Project!;

            if (ModelState.IsValid)
            {
                //***** Method to get the companyId
                int companyId = User.Identity!.GetCompanyId();
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

                // Add PM if one is selected
                if (!string.IsNullOrEmpty(viewModel.PMId))
                {
                    BTUser newPM = await _userManager.FindByIdAsync(viewModel.PMId);
                    await _projectService.AddProjectManagerAsync(newPM, project.Id);
                }
                else
                {
                    await _projectService.RemoveProjectManagerAsync(project.Id);
                }
                return RedirectToAction(nameof(AllProjects));
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            return View(project);
        }


        // GET: Projects/Edit/5
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            List<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id.Value);

            // use Project Service to create new AssignPMViewModel
            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id.Value),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };

            if (viewModel.Project == null)
            {
                return NotFound();
            }

            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            ViewData["ProjectManagers"] = new SelectList(projectManagers, "Id", "Name");
            ViewData["CurrentProjectManager"] = currentPM;

            return View(viewModel);
        }


        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Edit(AssignPMViewModel viewModel)
        {
            Project project = viewModel.Project!;

            if (ModelState.IsValid)
            {
                // Update time for Postgres so a cast of Date types isn't attempted
                project.Created = PostgresDate.Format(project.Created);
                project.StartDate = PostgresDate.Format(project.StartDate);
                project.EndDate = PostgresDate.Format(project.EndDate);

                try
                {
                    if (project.ImageFormFile != null)
                    {
                        project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                        project.ImageFileType = project.ImageFormFile.ContentType;
                    }
                    // use Project Service to update before returning
                    await _projectService.UpdateProjectAsync(project);

                    if (!string.IsNullOrEmpty(viewModel.PMId))
                    {
                        BTUser newPM = await _userManager.FindByIdAsync(viewModel.PMId);
                        await _projectService.AddProjectManagerAsync(newPM, project.Id);
                    }
                    else
                    {
                        await _projectService.RemoveProjectManagerAsync(project.Id);
                    }

                    return RedirectToAction("AllProjects");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await ProjectExists(project.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            ViewData["ProjectPriorityId"] = new SelectList(await _projectService.GetProjectPrioritiesAsync(), "Id", "Name");
            return RedirectToAction("Edit"); // was return View(project);
        }


        // GET: Projects/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            var project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        // POST: Projects/ArchiveConfirmed/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project != null)
            {
                await _projectService.ArchiveProjectAsync(project);
            }            

            return RedirectToAction(nameof(AllProjects));
        }


        // GET: Projects/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            var project = await _projectService.GetProjectByIdAsync(id.Value);

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }


        // POST: Projects/RestoreConfirmed/5
        [HttpPost, ActionName("Restore")]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int id)
        {
            if (id == 0)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();
            var project = await _projectService.GetProjectByIdAsync(id);

            if (project != null)
            {
                await _projectService.RestoreProjectAsync(project);
            }

            return RedirectToAction(nameof(AllProjects));
        }


        // TEST for Projects
        private async Task<bool> ProjectExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
            return (await _projectService.GetAllProjectsByCompanyIdAsync(companyId)).Any(p => p.Id == id);
        }
    }
}
