using Microsoft.AspNetCore.Identity;
using BugTrackerMVC.Models;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BugTrackerMVC.Extensions
{
    public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager,
                                            RoleManager<IdentityRole> roleManager, 
                                            IOptions<IdentityOptions> optionsAccessor)
            : base(userManager, roleManager, optionsAccessor)  // Accessing parent class with base
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }
}
