using BugTrackerMVC.Controllers;
using BugTrackerMVC.Data;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BugTrackerMVC.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        private readonly IBTProjectService _projectService;

        public BTTicketService(ApplicationDbContext context, IBTRolesService roleService, IBTProjectService projectService)
        {
            _context = context;
            _rolesService = roleService;
            _projectService = projectService;
        }

        //**** For testing Admin accounts
        public async Task<List<Ticket>> GetAllTicketsAsync()
        {
            try
            {
                return await _context.Tickets.Include(t => t.Project)
                                             .Include(t => t.Project!.Members)
                                             .Include(t => t.TicketPriority)
                                             .Include(t => t.TicketType)
                                             .Include(t => t.TicketStatus)
                                             .Include(t => t.DeveloperUser)
                                             .Include(t => t.SubmitterUser)                                            
                                             .ToListAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        //****

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {   
                Ticket? ticket = new();
                ticket = await _context.Tickets
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.Comments)
                                       .Include(t => t.Project)
                                       .Include(t => t.History)
                                       .Include(t => t.SubmitterUser)
                                       .Include(t => t.TicketPriority)
                                       .Include(t => t.TicketStatus)
                                       .Include(t => t.TicketType)
                                       .FirstOrDefaultAsync(m => m.Id == ticketId);
                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketType>> GetTicketTypesAsync()
        {
            try
            {
                List<TicketType> ticketTypes = new();
                ticketTypes = await _context.TicketTypes.ToListAsync();
                return ticketTypes; 
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketPriority>> GetTicketPrioritiesAsync()
        {
            try
            {
                List<TicketPriority> priorities = new();
                priorities = await _context.TicketPriorities.ToListAsync();
                return priorities;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<TicketStatus>> GetTicketStatusesAsync()
        {
            try
            {
                List<TicketStatus> statuses = new();
                statuses = await _context.TicketStatuses.ToListAsync();
                return statuses;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Ticket>> GetTicketsByUserIdAsync(string userId, int companyId)
        {
            BTUser? btUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
            List<Ticket>? tickets = (await _projectService.GetAllProjectsByCompanyIdAsync(companyId))
                                                    .Where(p => p.Archived == false)
                                                    .SelectMany(p => p.Tickets!)
                                                    .Where(t => !(t.Archived | t.ArchivedByProject))
                                                    .ToList();

            try
            {
                if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Admin)))
                {
                    return tickets;
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Developer)))
                {
                    return tickets.Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Submitter)))
                {
                    return tickets.Where(t => t.SubmitterUserId == userId).ToList();
                }
                else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
                {
                    List<Ticket>? projectTickets = (await _projectService.GetUserProjectsAsync(userId))
                                                    .SelectMany(t => t.Tickets!)
                                                    .Where(t => t.Archived | t.ArchivedByProject)
                                                    .ToList();
                    List<Ticket>? submittedTickets = tickets.Where(t => t.SubmitterUserId == userId).ToList();
                    return tickets = projectTickets.Concat(submittedTickets).ToList();
                }

                return tickets;

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByDeveloperIdAsync(string userId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(t => t.DeveloperUserId == userId)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.Project)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByProjectIdAsync(int projectId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(t => t.ProjectId == projectId)
                                                     .Include(t => t.DeveloperUser)
                                                     .Include(t => t.Project)
                                                     .Include(t => t.SubmitterUser)
                                                     .Include(t => t.TicketPriority)
                                                     .Include(t => t.TicketStatus)
                                                     .Include(t => t.TicketType)
                                                     .ToListAsync();
                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetAllTicketsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(t => t.Project!.CompanyId == companyId)
                                                             .Include(t => t.Project)
                                                             .Include(t => t.TicketPriority)
                                                             .Include(t => t.TicketType)
                                                             .Include(t => t.TicketStatus)
                                                             .Include(t => t.DeveloperUser)
                                                             .Include(t => t.SubmitterUser)
                                                             .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<Ticket> GetTicketAsNoTrackingAsync(int ticketId, int companyId)
        {
            try
            {
				Ticket? ticket = new();
				ticket = await _context.Tickets
                                       .Include(t => t.Project)
                                            .ThenInclude(p => p.Company)
                                       .Include(t => t.Attachments)
									   .Include(t => t.DeveloperUser)
									   .Include(t => t.Comments)
									   .Include(t => t.History)
									   .Include(t => t.SubmitterUser)
									   .Include(t => t.TicketPriority)
									   .Include(t => t.TicketStatus)
									   .Include(t => t.TicketType)
                                       .AsNoTracking()
									   .FirstOrDefaultAsync(m => m.Id == ticketId && m.Project!.CompanyId == companyId && m.Archived == false);

				return ticket;
            }
            catch (Exception)
            {

                throw;
            }
        }

		public async Task<List<Ticket>> GetArchivedTicketsByDeveloperIdAsync(string userId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets
                                       .Where(t => t.DeveloperUserId == userId && t.Archived == true)
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.Project)
                                       .Include(t => t.SubmitterUser)
                                       .Include(t => t.TicketPriority)
                                       .Include(t => t.TicketStatus)
                                       .Include(t => t.TicketType)
                                       .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
         
        public async Task AssignDeveloperAsync(Ticket ticket, string developerUserId)
        {
            try
            {
                ticket.DeveloperUserId = developerUserId;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        
        public async Task AddCommentAsync(TicketComment ticketComment)
        {
            try
            {
                _context.TicketComments.Add(ticketComment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task ArchiveTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = true;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RestoreTicketAsync(Ticket ticket)
        {
            try
            {
                ticket.Archived = false;
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Tickets.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
