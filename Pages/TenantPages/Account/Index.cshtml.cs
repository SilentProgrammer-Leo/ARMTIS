using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPages.Account
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<TenantAccountViewModel> TenantAccounts { get; set; } = new();

        public async Task OnGetAsync()
        {
            var tenants = await _context.Tenants
                .AsNoTracking()
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .ToListAsync();

            var users = await _userManager.Users
                .AsNoTracking()
                .ToListAsync();

            TenantAccounts = tenants.Select(tenant =>
            {
                var account = users.FirstOrDefault(
                    u => u.TenantID == tenant.TenantID);

                return new TenantAccountViewModel
                {
                    TenantID = tenant.TenantID,
                    FullName = $"{tenant.FirstName} {tenant.LastName}",
                    Username = account?.UserName,
                    HasAccount = account != null
                };
            }).ToList();
        }

        public class TenantAccountViewModel
        {
            public int TenantID { get; set; }

            public string FullName { get; set; } = string.Empty;

            public string? Username { get; set; }

            public bool HasAccount { get; set; }
        }
    }
}
