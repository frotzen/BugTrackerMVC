using BugTrackerMVC.Data;
using BugTrackerMVC.Models;
using BugTrackerMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Collections;
//using X.PagedList;

namespace BugTrackerMVC.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;

        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<List<Project>> GetAllProjectsByCompanyIdAsync(int companyId)
        {
            try
            {
                List<Project> projects = await _context.Projects
                                        .Where(p => p.CompanyId == companyId)
                                        .ToListAsync();
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
                                         .Where(p => p.CompanyId == companyId)
                                         .Where(b => b.Archived == true)
                                         .ToListAsync();
                return projects;
            }
            catch (Exception)
            {
                throw;
            }
        }
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
        public async Task<Project> GetProjectByIdAsync(int projectId)
        {
            try
            {
                Project? project = new();
                project = await _context.Projects.FindAsync(projectId);
                return project;
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
                _context.Update(project);
                await _context.SaveChangesAsync();

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
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }


        // incomplete: returning the project priorities as a List?
        //      locate in ProjectsController.cs GET: Create()
        //  *** currently breaks projects controller
        public async Task<IEnumerable> GetProjectPriorities()
        {
            IEnumerable priorities = await _context.ProjectPriorities.ToListAsync();
            return priorities;
        }
    }
}
