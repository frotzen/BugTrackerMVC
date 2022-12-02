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

namespace BugTrackerMVC.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTTicketService _ticketService;

        public TicketsController(ApplicationDbContext context, UserManager<BTUser> userManager,
                                 IBTTicketService ticketService, IBTProjectService projectService)
        {
            _context = context;
            _userManager = userManager;
            _projectService = projectService;
            _ticketService = ticketService;
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
            //  int companyId = User.Identity!.GetCompanyId();
            //  GetAllTicketsByDeveloperIdAsync(string userId)
            //

            string userId = (await _userManager.GetUserAsync(User)).Id;
            List<Ticket> tickets = new();

            if (User.IsInRole(nameof(BTRoles.Admin)))
            {
                //  !!!!!!!*****  take another look at this  *****!!!!!!!
                // call GetAllTickets from service
                // 
                tickets = await _ticketService.GetAllTicketsByDeveloperIdAsync(userId);
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

            //ViewData["DeveloperUserId"] = new SelectList(_context.Set<BTUser>(), "Id", "FullName");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");

            return View();
        }

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,ProjectId,TicketTypeId,TicketPriorityId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            ticket.SubmitterUserId = _userManager.GetUserId(User);

            if (ModelState.IsValid)
            {
                // Update time for Postgres so a cast of Date types isn't attempted
                ticket.Created = PostgresDate.Format(DateTime.Now);
                ticket.Updated = PostgresDate.Format(DateTime.Now);

                
                ticket.TicketStatusId = (await _context.TicketStatuses
                                .FirstOrDefaultAsync(s => s.Name  == nameof(BTTicketStatuses.New)))!.Id;

                await _ticketService.AddTicketAsync(ticket);

                return RedirectToAction(nameof(Index));
            }
            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

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

            // SelectList(table, dataValue, dataText, selectvalue)
            // Param list 2nd & 3rd values are for actual dataValue & display dataText, respectively
            // dataValue is submitted by the form, dataText shows up in the html selector element
            //   using FullName for 3rd value displays full name for both types of users

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            ViewData["SubmitterUserId"] = ticket.SubmitterUserId;
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);

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

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "FullName", ticket.DeveloperUserId);
            //ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = ticket.SubmitterUserId;
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
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
