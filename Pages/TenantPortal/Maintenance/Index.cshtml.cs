using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Maintenance
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

        public List<MaintenanceRequest> MaintenanceRequests { get; set; }
            = new List<MaintenanceRequest>();

        public int TotalRequests { get; set; }

        public int PendingRequests { get; set; }

        public int InProgressRequests { get; set; }

        public int CompletedRequests { get; set; }

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

            int tenantID = user.TenantID.Value;

            // Get ONLY this tenant's maintenance requests
            MaintenanceRequests = await _context.MaintenanceRequests
                .AsNoTracking()
                .Include(m => m.Unit)
                .Where(m => m.TenantID == tenantID)
                .OrderByDescending(m => m.RequestDate)
                .ThenByDescending(m => m.MaintenanceRequestID)
                .ToListAsync();

            TotalRequests = MaintenanceRequests.Count;

            PendingRequests = MaintenanceRequests.Count(
                m => m.Status == "Pending");

            InProgressRequests = MaintenanceRequests.Count(
                m => m.Status == "In Progress");

            CompletedRequests = MaintenanceRequests.Count(
                m => m.Status == "Completed");

            return Page();
        }
    }
}