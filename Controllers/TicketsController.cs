using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using BugTrackerMVC.Helper;
using Microsoft.AspNetCore.Authorization;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models.ViewModels;

namespace BugTrackerMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTTicketHistoryService _historyService;

        public TicketsController(UserManager<BTUser> userManager, IBTCompanyService companyService,
                                 IBTTicketService ticketService, IBTFileService fileService,
                                 IBTProjectService projectService, IBTRolesService rolesService,
                                 IBTTicketHistoryService historyService)
        {
            _userManager = userManager;
            _fileService = fileService;
            _projectService = projectService;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _historyService = historyService;
            _companyService = companyService;
        }

        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            //  int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            //  GetAllTicketsByDeveloperIdAsync(string userId)
            //
            BTUser btUser = await _userManager.GetUserAsync(User);
            List<Ticket> tickets = await _ticketService.GetTicketsByUserIdAsync(btUser.Id, btUser.CompanyId);

            return View(tickets);
        }


        // GET: Tickets/UnassignedTickets
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            //  int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            //  GetAllTicketsByDeveloperIdAsync(string userId)
            //
            int companyId = User.Identity!.GetCompanyId();
            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            //  !!!!!!!*****  take another look at this  *****!!!!!!!
            // call Get All Tickets from service
            // 
            tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            tickets = tickets.Where(t => t.DeveloperUserId == null).ToList();

            return View(tickets);
        }


        // Get: Tickets/ArchivedTickets
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                tickets = await _ticketService.GetAllArchivedTicketsByCompanyIdAsync(companyId);
            }
            else
            {
                tickets = await _ticketService.GetArchivedTicketsByDeveloperIdAsync(userId);
            }

            return View(tickets);
        }


        // Get: Tickets/AllTickets
        public async Task<IActionResult> AllTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            if (User.IsInRole(nameof(BTRoles.Admin)) || User.IsInRole(nameof(BTRoles.ProjectManager)))
            {
                tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            }
            else if (User.IsInRole(nameof(BTRoles.Developer)))
            {
                tickets = await _ticketService.GetAllTicketsByDeveloperIdAsync(userId);
            }
            else
            {
                tickets = await _ticketService.GetTicketsByUserIdAsync(userId, companyId);
            }

            return View(tickets);
        }


        // GET: Ticket/AssignDevelper/5
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignDeveloper(int id)
        {
            AssignDeveloperViewModel model = new();

            model.Ticket = await _ticketService.GetTicketByIdAsync(id);
            model.Developers = new SelectList(await _projectService.GetProjectMembersByRoleAsync(model.Ticket.ProjectId, nameof(BTRoles.Developer)),
                                   "Id", "FullName");

            return View(model);
        }


        // POST: Tickets/AssignDeveloper/5
        [HttpPost]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(AssignDeveloperViewModel model)
        {
            if (model.DeveloperId != null)
            {
                BTUser btUser = await _userManager.GetUserAsync(User);
                int companyId = btUser.CompanyId;

                // get ticket before assigning developer
                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id, companyId);

                try
                {
                    await _ticketService.AssignDeveloperAsync(model.Ticket!, model.DeveloperId);
                }
                catch (Exception)
                {

                    throw;
                }

                // get ticket after assigning developer then call history service
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(model.Ticket!.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, btUser.Id);

                return RedirectToAction(nameof(Details), new { id = model.Ticket.Id });
            }

            return RedirectToAction(nameof(AssignDeveloper), new { id = model.Ticket!.Id });
        }


        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int id)
        {
            TicketDetailsViewModel viewModel = new();

            if (id == null)
            {
                return NotFound();
            }

            viewModel.Ticket = await _ticketService.GetTicketByIdAsync(id);

            if (viewModel.Ticket == null)
            {
                return NotFound();
            }

            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Ticket.ProjectId);
            viewModel.TicketComments = await _ticketService.GetTicketCommentsAsync(id);

            return View(viewModel);
        }


        // POST: Tickets/Details
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Details(int id, TicketDetailsViewModel viewModel)
        {
            if (id != 0)
            {
                TicketComment comment = new();
                try
                {
                    comment.Comment = viewModel.TicketComment!.Comment;
                    comment.Created = PostgresDate.Format(DateTime.Now);
                    comment.UserId = (await _userManager.GetUserAsync(User)).Id;
                    comment.TicketId = viewModel.Ticket!.Id;

                    await _ticketService.AddCommentAsync(comment);

                    // Add history
                    await _historyService.AddHistoryAsync(comment.TicketId, nameof(TicketComment), comment.UserId);
                    return RedirectToAction("Details", new { id = comment!.TicketId });
                }
                catch (Exception)
                {

                    throw;
                }
            }
            return RedirectToAction(nameof(Details));
        }


        // POST: AddTicketAttachment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                try
                {
                    ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                    ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                    ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                    ticketAttachment.Created = PostgresDate.Format(DateTime.Now);
                    ticketAttachment.UserId = _userManager.GetUserId(User);

                    await _ticketService.AddTicketAttachmentAsync(ticketAttachment);

                    // History
                    await _historyService.AddHistoryAsync(ticketAttachment.TicketId, nameof(TicketAttachment), ticketAttachment.UserId);
                }
                catch (Exception)
                {
                    throw;
                }

                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }


        // GET: ShowFile
        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName).Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }


        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            string userId = (await _userManager.GetUserAsync(User)).Id;
            int companyId = User.Identity!.GetCompanyId();
            List<Project> projects = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                projects = await _projectService.GetAllProjectsByCompanyIdAsync(companyId);
            }
            else
            {
                projects = await _projectService.GetUserProjectsAsync(userId);
            }

            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users
            //   4th parameter is for displaying selected item, this is a new ticket, so not needed.

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            ViewData["DeveloperUserId"] = new SelectList(developers, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(await _companyService.GetMembersAsync(companyId), "Id", "FullName");
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name");

            return View();
        }


        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,DeveloperUserId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            BTUser btUser = await _userManager.GetUserAsync(User);
            int companyId = User.Identity!.GetCompanyId();
            ModelState.Remove("SubmitterUserId");

            if (ModelState.IsValid)
            {
                try
                {
                    ticket.SubmitterUserId = btUser.Id;

                    // Update time for Postgres so a cast of Date types isn't attempted
                    ticket.Created = PostgresDate.Format(DateTime.Now);
                    ticket.Updated = PostgresDate.Format(DateTime.Now);

                    // Set new ticket status to New using Enums
                    ticket.TicketStatusId = (await _ticketService.GetTicketStatusesAsync())
                                    .FirstOrDefault(s => s.Name == nameof(BTTicketStatuses.New))!.Id;

                    // Add ticket and get user's companyId
                    await _ticketService.AddTicketAsync(ticket);

                    //*****
                    // Add History record here.
                    Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                    await _historyService.AddHistoryAsync(null!, newTicket, btUser.Id);

                    // TODO: Notifications
                }
                catch (Exception)
                {

                    throw;
                }

                return RedirectToAction(nameof(AllTickets));
            }
            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users


            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            ViewData["DeveloperUserId"] = new SelectList(developers, "Id", "FullName");
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            // ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }


        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // call ticket service
            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            // SelectList(table, dataValue, dataText, selectvalue)
            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            ViewData["DeveloperUserId"] = new SelectList(developers, "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["SubmitterUserId"] = ticket.SubmitterUserId;
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);

            return View(ticket);
        }


        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            int companyId = User.Identity!.GetCompanyId();
            string userId = _userManager.GetUserId(User);
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                // check if archived
                if (ticket.Archived)
                {
                    // maybe make an alert and return to view?
                    return RedirectToAction(nameof(ArchivedTickets));
                }

                Ticket oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                try
                {
                    // Format dates
                    ticket.Created = PostgresDate.Format(ticket.Created);
                    ticket.Updated = PostgresDate.Format(DateTime.Now);

                    // Check Description for HTML tags <>
                    // Remove HTML tags from Description if necessary
                    // *Quill puts tags around things
                    string regexDescription = ticket.Description!;
                    var regexTags = new System.Text.RegularExpressions.Regex("<[^>]*>");
                    regexDescription = regexTags.Replace(regexDescription, "");

                    if (regexDescription != ticket.Description)
                    {
                        ticket.Description = regexDescription;
                    }

                    await _ticketService.UpdateTicketAsync(ticket);
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!await TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                // Add ticket history
                Ticket newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);
                await _historyService.AddHistoryAsync(oldTicket, newTicket, userId);


                return RedirectToAction(nameof(AllTickets));
            }

            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users

            ViewData["DeveloperUserId"] = new SelectList(await _companyService.GetMembersAsync(companyId), "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = ticket.SubmitterUserId;
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }


        // GET: Tickets/Archive/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Archive(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // POST: Tickets/ArchiveConfirmed/5
        [HttpPost, ActionName("Archive")]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int? id)
        {
            if (id == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Ticket'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }

            return RedirectToAction(nameof(AllTickets));
        }


        // GET: Tickets/Restore/5
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Restore(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }


        // POST: Tickets/RestoreConfirmed/5
        [HttpPost, ActionName("Restore")]
        [Authorize(Roles = "Admin, ProjectManager")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConfirmed(int? id)
        {
            if (id == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Ticket'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id.Value);

            if (ticket != null)
            {
                await _ticketService.RestoreTicketAsync(ticket);
            }

            return RedirectToAction(nameof(AllTickets));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
            return (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Any(t => t.Id == id);
        }
    }
}
