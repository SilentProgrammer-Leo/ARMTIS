using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Dashboard
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

        public int TenantID { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (user.TenantID == null)
            {
                return Forbid();
            }

            TenantID = user.TenantID.Value;

            Tenant = await _context.Tenants
                .Include(t => t.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantID == TenantID);

            if (Tenant == null)
            {
                return NotFound();
            }

            return Page();
        }
    }
}
