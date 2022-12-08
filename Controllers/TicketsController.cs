using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using BugTrackerMVC.Helper;
using Microsoft.AspNetCore.Authorization;
using BugTrackerMVC.Services;
using BugTrackerMVC.Enums;
using System.ComponentModel.Design;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models.ViewModels;

namespace BugTrackerMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager,
                                 IBTTicketService ticketService, IBTProjectService projectService,
                                 IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
            _rolesService = rolesService;
        }

        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Ticket> tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            return View(tickets);
        }

        // GET: Tickets/MyTickets
        public async Task<IActionResult> MyTickets()
        {
            //  int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            //  GetAllTicketsByDeveloperIdAsync(string userId)
            //
            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                //  !!!!!!!*****  take another look at this  *****!!!!!!!
                // call Get All Tickets from service
                // 
                tickets = await _ticketService.GetAllTicketsByDeveloperIdAsync(userId);
            }
            else
            {
                tickets = await _ticketService.GetAllTicketsByDeveloperIdAsync(userId);
            }

            return View(tickets);
        }
        // GET: Tickets/UnassignedTickets
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
            //  int companyId = (await _userManager.GetUserAsync(User)).CompanyId;
            //  GetAllTicketsByDeveloperIdAsync(string userId)
            //

            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                //  !!!!!!!*****  take another look at this  *****!!!!!!!
                // call Get All Archived Tickets from service
                // 
                tickets = await _ticketService.GetArchivedTicketsByDeveloperIdAsync(userId);
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
            //

            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                //  !!!!!!!*****  take another look at this  *****!!!!!!!
                // call GetAllTicketsAsync() from _ticketService for testing
                //  put GetAllTicketsByCompanyIdAsyn(companyId) back when done

                 tickets = await _ticketService.GetAllTicketsAsync();
                //tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);

            }
            else if(User.IsInRole(nameof(BTRoles.ProjectManager)))
            {
                tickets = await _ticketService.GetAllTicketsByCompanyIdAsync(companyId);
            }
            else
            {
                tickets = await _ticketService.GetAllTicketsByDeveloperIdAsync(userId);
            }

            return View(tickets);
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
            viewModel.TicketComments = await _context.TicketComments.Where(c => c.TicketId == id).ToListAsync();

            return View(viewModel);
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

            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer) , companyId);

            ViewData["DeveloperUserId"] = new SelectList(developers, "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName");
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
            ModelState.Remove("SubmitterUserId");            

            if (ModelState.IsValid)
            {
                ticket.SubmitterUserId = _userManager.GetUserId(User);

                // Update time for Postgres so a cast of Date types isn't attempted
                ticket.Created = PostgresDate.Format(DateTime.Now);
                ticket.Updated = PostgresDate.Format(DateTime.Now);
                
                // Set new ticket status to New using Enums
                ticket.TicketStatusId = (await _context.TicketStatuses
                                .FirstOrDefaultAsync(s => s.Name  == nameof(BTTicketStatuses.New)))!.Id;

                // Add ticket
                await _ticketService.AddTicketAsync(ticket);

                return RedirectToAction(nameof(Index));
            }
            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users

            int companyId = User.Identity!.GetCompanyId();
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
            if (id != ticket.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {

                try
                {
                    // Format dates
                    ticket.Created = PostgresDate.Format(ticket.Created);
                    ticket.Updated = PostgresDate.Format(DateTime.Now);

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
                return RedirectToAction(nameof(Index));
            }

            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users

            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = ticket.SubmitterUserId;
            ViewData["TicketPriorityId"] = new SelectList(await _ticketService.GetTicketPrioritiesAsync(), "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(await _ticketService.GetTicketStatusesAsync(), "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(await _ticketService.GetTicketTypesAsync(), "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Archive/5
        public async Task<IActionResult> Archive(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);
                
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Archive/5
        [HttpPost, ActionName("Archive")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ArchiveConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Ticket'  is null.");
            }

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id);

            if (ticket != null)
            {
                await _ticketService.ArchiveTicketAsync(ticket);
            }            
            
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TicketExists(int id)
        {
            int companyId = User.Identity!.GetCompanyId();
            return (await _ticketService.GetAllTicketsByCompanyIdAsync(companyId)).Any(e => e.Id == id);
        }
    }
}
