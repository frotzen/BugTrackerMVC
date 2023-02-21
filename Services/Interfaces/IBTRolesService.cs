using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTRolesService
    {
        // methods do not have async modifier here, only in implementation
        public Task<bool> AddUserToRoleAsync(BTUser user, string roleName);
        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns>
        /// List of BTUser
        /// </returns>
        
        public Task<bool> AddUserToRolesAsync(BTUser user, IEnumerable<string> roleNames);
        /// <summary>
        /// Get All Roles
        /// </summary>
        /// <returns>
        /// List of BTUser
        /// </returns>

        public Task<List<IdentityRole>> GetRolesAsync();
        /// <summary>
        /// Get roles for a BTUser
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// IEnumerable of string
        /// </returns>
        public Task<IEnumerable<string>> GetUserRolesAsync(BTUser user);
        /// <summary>
        /// Remove a user from a role
        /// </summary>
        /// <param name="user"></param>
        /// <returns>
        /// bool</returns>
        public Task<bool> RemoveUserFromRoleAsync(BTUser user, string roleName);
        public Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId);
        public Task<bool> IsUserInRoleAsync(BTUser member, string roleName);
        /// <summary>
        /// Remove a user from a list of roles
        /// </summary>
        /// <param name="user"></param>
        /// <param name="roleNames"></param>
        /// <returns>
        /// bool
        /// </returns>
        public Task<bool> RemoveUserFromRolesAsync(BTUser user, IEnumerable<string> roleNames);
    }
}
