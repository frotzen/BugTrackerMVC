using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTProjectService
    {
        // Needs to be implemented for Interface, No logic, all is public, all
        // methods have to be declared here for class intended to be a service
        //
        // methods do not have async modifier here, only in implementation

        /* Getters */
        public Task<Project> GetProjectByIdAsync(int projectId);
        public Task<List<Project>> GetUserProjectsAsync(string userId);
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetAllUnassignedProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);
        public Task<List<BTUser>> GetProjectMembersByRoleAsync(int projectId, string role);
        public Task<List<BTUser>> GetAllProjectMembersExceptPMAsync(int projectId);
        public Task<List<Project>> GetAllProjectsByPriorityAsync(int companyId, string priorityName);
        public Task<List<ProjectPriority>> GetProjectPrioritiesAsync();
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
