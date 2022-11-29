using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace BugTrackerMVC.Services
{
    public class BTRolesService : IBTRolesService
    {
        private readonly UserManager<BTUser> _userManager;
        private readonly ApplicationDbContext _context;

        public BTRolesService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<List<BTUser>> GetUsersInRoleAsync(string roleName, int companyId)
        {
            try
            {
                List<BTUser> results = new();
                List<BTUser> users = new();

                users = (await _userManager.GetUsersInRoleAsync(roleName)).ToList();
                results = users.Where(i => i.CompanyId == companyId).ToList();

                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> IsUserInRoleAsync(BTUser member, string roleName)
        {
            try
            {
                bool results = await _userManager.IsInRoleAsync(member, roleName);
                return results;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
