using BugTrackerMVC.Models;
using System.Collections;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTProjectService
    {
        // Needs to be implemented for Interface, No logic, all is public, all
        // methods have to be implemented here for class intended to be a service
        public Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId);
        public Task<List<Project>> GetArchivedProjectsByCompanyIdAsync(int companyId);
        public Task AddProjectAsync(Project project);

        // Todo: return List of project priorities?
        public Task<IEnumerable> GetProjectPriorities();

        public Task<Project> GetProjectByIdAsync(int projectId);
        public Task UpdateProjectAsync(Project project);
        public Task ArchiveProjectAsync(Project project);
        public Task RestoreProjectAsync(Project project);

    }
}
