﻿using BugTrackerMVC.Models;

namespace BugTrackerMVC.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<Company> GetCompanyInfoAsync(int companyId);
        public Task<List<BTUser>> GetMembersAsync(int companyId);
    }
}
