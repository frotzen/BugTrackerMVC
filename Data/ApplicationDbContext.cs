﻿using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Models;

namespace BugTrackerMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<BTUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public virtual DbSet<Company> Companies { get; set; } = default!;
        public virtual DbSet<Invite> Invites { get; set; } = default!;
        public virtual DbSet<Notification> Notifications { get; set; } = default!;
        public virtual DbSet<Project> Projects { get; set; } = default!;
        public virtual DbSet<Ticket> Tickets { get; set; } = default!;
        public virtual DbSet<TicketComment> TicketComments { get; set; } = default!;
        public virtual DbSet<TicketAttachment> TicketAttachments { get; set; } = default!;
        public virtual DbSet<TicketHistory> TicketHistories { get; set; } = default!;
        public virtual DbSet<TicketPriority> TicketPriorities { get; set; } = default!;
        public virtual DbSet<TicketType> TicketTypes { get; set; } = default!;
        public virtual DbSet<TicketStatus> TicketStatuses { get; set; } = default!;
        public virtual DbSet<ProjectPriority> ProjectPriorities { get; set; } = default!;
        public virtual DbSet<NotificationType> NotificationTypes { get; set; } = default!;



        //public DbSet<Company> Company { get; set; }
        //public DbSet<Invite> Invite { get; set; }
        //public DbSet<Notification> Notification { get; set; }
        //public DbSet<Project> Project { get; set; }
        //public DbSet<Ticket> Ticket { get; set; }
        //public DbSet<TicketComment> TicketComment { get; set; }
        //public DbSet<TicketAttachment> TicketAttachment { get; set; }
        //public DbSet<TicketHistory> TicketHistory { get; set; }
    }
}