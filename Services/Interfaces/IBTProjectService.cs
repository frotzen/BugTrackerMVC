using BugTrackerMVC.Models;
using Org.BouncyCastle.Bcpg;
using System.Collections;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTProjectService
    {
        // Needs to be implemented for Interface, No logic, all is public, all
        // methods have to be implemented here for class intended to be a service
        
        /* Getters */
        public Task<Project> GetProjectByIdAsync(int projectId, int companyId);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);
        public Task<IEnumerable<ProjectPriority>> GetProjectPrioritiesAsync();
        public Task<BTUser> GetProjectManagerAsync(int projectId);

        /* Setters */
        public Task AddProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);
        public Task ArchiveProjectAsync(Project project);
        public Task RestoreProjectAsync(Project project);
        public Task<bool> AddProjectManagerAsync(BTUser member, int projectId);
        public Task<bool> RemoveProjectManagerAsync(int projectId);
        public Task<bool> AddMemberToProjectAsync(BTUser member, int projectId);
        public Task<bool> RemoveMemberFromProjectAsync(BTUser member, int projectId);

 

    }
}
