using BugTrackerMVC.Data;
using BugTrackerMVC.Enums;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System.Data;

namespace BugTrackerMVC.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;

        public BTProjectService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        /****
         **     Getters
         ****/
        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project? project = new();
                project = await _context.Projects
                                        .Include(p => p.Company)
                                        .Include(p => p.Members)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.Comments)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.Attachments)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.History)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketPriority)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketStatus)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketType)
                                        .Include(p => p.ProjectPriority)
                                        .FirstOrDefaultAsync(p => p.Id == projectId);
                return project!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Project>> GetUserProjectsAsync(string userId)
        {
            try
            {
                List<Project>? projects = (await _context.Users
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.ProjectPriority)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Members)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.TicketType)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.TicketStatus)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.TicketPriority)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.Comments)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.Attachments)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.History)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.DeveloperUser)
                                        .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                                .ThenInclude(t => t.SubmitterUser)
                                        .FirstOrDefaultAsync(u => u.Id == userId))?
                                        .Projects.Where(p => p.Archived == false)
                                        .ToList();
                return projects!;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                        .Where(p => p.CompanyId == companyId)
                                        .Include(p => p.Company)
                                        .Include(p => p.ProjectPriority)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(c => c.Comments)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.Attachments)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.History)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketPriority)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketStatus)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketType)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.DeveloperUser)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.SubmitterUser)
                                        .Include(p => p.Members)
                                        .ToListAsync();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<Project>> GetAllUnassignedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                // get all unassigned projects where Members has no PM
                bool isMember = false;
                List<Project>? projects = new();
                List<Project>? projectList = await _context.Projects
                                                            .Include(p => p.Members)
                                                            .Include(p => p.Company)
                                                            .Include(p => p.ProjectPriority)
                                                            .Where(p => p.CompanyId == companyId)
                                                            .ToListAsync();
                foreach(var project in projectList)
                {
                    foreach(var member in project.Members)
                    {
                        if(await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                        {
                            isMember = true;
                            break;
                        }

                    }

                    if (isMember == true)
                    {
                        isMember = false;
                        continue;
                    }

                    else
                    {
                        projects.Add(project);
                    }
                }

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                         .Where(p => p.CompanyId == companyId && p.Archived == true)
                                        .Include(p => p.Company)
                                        .Include(p => p.ProjectPriority)
                                        .Include(p => p.Tickets)
                                            .ThenInclude(c => c.Comments)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.Attachments)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.History)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketPriority)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketStatus)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.TicketType)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.DeveloperUser)
                                        .Include(p => p.Tickets)
                                           .ThenInclude(c => c.SubmitterUser)
                                        .Include(p => p.Members)
                                        .ToListAsync();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName)
        {
            List<Project> projects = await GetAllProjectsByCompanyIdAsync(companyId);
            int priorityId = (await _context.ProjectPriorities
                                           .FirstOrDefaultAsync(p => p.Name == priorityName))!
                                           .Id;
            return projects.Where(p => p.ProjectPriorityId == priorityId).ToList();
        }

        public async Task<List<ProjectPriority>> GetProjectPrioritiesAsync()
        {
            return await _context.ProjectPriorities.ToListAsync();
        }

        /****
         **     Setters
         ****/
        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task ArchiveProjectAsync(Project project)
        {
            try
            {
                project.Archived = true;
                await UpdateProjectAsync(project);

                // Archive Project tickets
                foreach(Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = true;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task RestoreProjectAsync(Project project)
        {
            try
            {
                project.Archived = false;
                await UpdateProjectAsync(project);

                // Archive Project tickets
                foreach (Ticket ticket in project.Tickets)
                {
                    ticket.ArchivedByProject = false;
                    _context.Update(ticket);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }


        /* Project Manager methods */
        public async Task<BTUser> GetProjectManagerAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }
                return null!;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> AddProjectManagerAsync(BTUser member, int projectId)
        {
            // Adding a Project Manager will add them to this project only
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(member.Id);

                // Remove current ProjectManager
                if (currentPM != null)
                {
                    await RemoveProjectManagerAsync(projectId);
                }

                // Add selected ProjectManager
                try
                {
                    await AddMemberToProjectAsync(selectedPM!, projectId);
                    return true;
                }

                catch (Exception)
                {

                    throw;
                }
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> RemoveProjectManagerAsync(int projectId)
        {
            // Removing a Project Manager will remove them from this project only
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);

                if (currentPM != null)
                {
                    return await RemoveMemberFromProjectAsync(currentPM, projectId);
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        /* Project Member methods */
        public async Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);
                List<BTUser> members = new();

                foreach (var user in project!.Members)
                {
                    if (await _rolesService.IsUserInRoleAsync(user, role))
                    {
                        members.Add(user);
                    }
                }
                return members;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members)
                                                          .FirstOrDefaultAsync(p => p.Id == projectId);
                List<BTUser> members = new();

                foreach (var user in project!.Members)
                {
                    if (!await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.ProjectManager)))
                    {
                        members.Add(user);
                    }
                }
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> AddMemberToProjectAsync(BTUser member, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);
                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId);
                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);
                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();
                    return true;
                }
                return false;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
