using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Maintenance
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetailsModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public MaintenanceRequest MaintenanceRequest { get; set; }
            = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    m => m.MaintenanceRequestID == id.Value);

            if (request == null)
            {
                return NotFound();
            }

            MaintenanceRequest = request;

            return Page();
        }
    }
}