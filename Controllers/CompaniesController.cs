using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using Microsoft.AspNetCore.Identity;
using BugTrackerMVC.Models.ViewModels;
using BugTrackerMVC.Extensions;
using BugTrackerMVC.Services.Interfaces;
using BugTrackerMVC.Services;
using Microsoft.AspNetCore.Authorization;

namespace BugTrackerMVC.Controllers
{
    [Authorize(Roles = "Admin")]
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;
       

        public CompaniesController(ApplicationDbContext context, UserManager<BTUser> userManager,
                                    IBTCompanyService companyService, IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _companyService = companyService;
            _rolesService = rolesService;
           
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: ManageUserRoles
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // Add instance of ViewModel
            List<ManageUserRolesViewModel> model = new();

            // Get CompanyId
            int companyId = User.Identity!.GetCompanyId();

            // Get all company users
            List<BTUser> members = await _companyService.GetMembersAsync(companyId);

            // loop over users to populate ViewModel
            // instantiate ViewModel
            // use _roleService
            // create multiselect
            // viewmodel to model
            string btUserId = _userManager.GetUserId(User);
            foreach (BTUser member in members)
            {
                if (string.Compare(btUserId, member.Id) != 0)
                {
                    ManageUserRolesViewModel viewModel = new();

                    IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(member);

                    viewModel.BTUser = member;
                    viewModel.Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Name", "Name", currentRoles);

                    model.Add(viewModel);
                }
            }

            // return model to view
            return View(model);
        }

        // POST: ManageUserRoles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel model)
        {
            // get companyId
            int companyId = User.Identity!.GetCompanyId();

            // instantiate BTUser
            BTUser? btUser = (await _companyService.GetMembersAsync(companyId)).FirstOrDefault(m => m.Id == model.BTUser!.Id);

            // get roles for that user
            IEnumerable<string> currentRoles = await _rolesService.GetUserRolesAsync(btUser!);

            // get selected roles for user
            string? selectedRole = model.SelectedRoles!.FirstOrDefault();

            // remove current roles and add new role
            if (!string.IsNullOrEmpty(selectedRole))
            {
                if (await _rolesService.RemoveUserFromRolesAsync(btUser!, currentRoles))
                {
                    await _rolesService.AddUserToRoleAsync(btUser!, selectedRole);
                }
            }

            // navigate
            return RedirectToAction(nameof(ManageUserRoles));
        }



    }
}
