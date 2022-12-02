﻿using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task<Ticket> GetTicketByIdAsync(int ticketId);
        public Task<List<Ticket>> GetAllTicketsByDeveloperIdAsync(string userId);
        public Task<List<Ticket>> GetArchivedTicketsByDeveloperIdAsync(string userId);
        public Task AddTicketAsync(Ticket ticket);
        public Task UpdateTicketAsync(Ticket ticket);
        public Task ArchiveTicketAsync(Ticket ticket);
        public Task RestoreTicketAsync(Ticket ticket);
    }
}
