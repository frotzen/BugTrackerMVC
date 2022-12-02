using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;

namespace BugTrackerMVC.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Ticket> GetTicketByIdAsync(int ticketId)
        {
            try
            {   
                Ticket? ticket = new();
                ticket = await _context.Tickets
                                       .Include(t => t.DeveloperUser)
                                       .Include(t => t.Project)
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
