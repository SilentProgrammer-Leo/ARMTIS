using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.TenantUnit
{
    [Authorize(Roles = "Tenant")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Tenant? Tenant { get; set; }

        public Unit? Unit { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the currently logged-in tenant account
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Make sure the account is linked to a tenant
            if (user.TenantID == null)
            {
                return Forbid();
            }

            // Get the tenant connected to this account
            Tenant = await _context.Tenants
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantID == user.TenantID.Value);

            if (Tenant == null)
            {
                return NotFound();
            }

            // Make sure the tenant has an assigned unit
            if (Tenant.UnitID == null)
            {
                return Page();
            }

            // Get ONLY the unit assigned to this tenant
            Unit = await _context.Units
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.UnitID == Tenant.UnitID);

            if (Unit == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}