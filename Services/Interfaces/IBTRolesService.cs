using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTRolesService
    {
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);

        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);
    }
}
