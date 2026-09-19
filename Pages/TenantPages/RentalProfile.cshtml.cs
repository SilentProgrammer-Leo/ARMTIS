using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPages
{
    public class RentalProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public RentalProfileModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public Tenant Tenant { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantID == id.Value);

            if (tenant == null)
            {
                return NotFound();
            }

            Tenant = tenant;

            return Page();
        }
    }
}