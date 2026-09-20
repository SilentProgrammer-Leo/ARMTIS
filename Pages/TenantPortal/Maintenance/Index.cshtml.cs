using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ARMTIS_Capstone_Project.Services;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Maintenance
{
    [Authorize(Roles = "Tenant")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public IndexModel(ApplicationDbContext context, UserManager<ApplicationUser> userManager, NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        public List<MaintenanceRequest> MaintenanceRequests { get; set; }
            = new List<MaintenanceRequest>();

        public int TotalRequests { get; set; }

        public int PendingRequests { get; set; }

        public int InProgressRequests { get; set; }

        public int CompletedRequests { get; set; }
        public int CancelledRequests { get; set; }

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

            CancelledRequests = MaintenanceRequests.Count(
                m => m.Status == "Cancelled");

            return Page();
        }

        public async Task<IActionResult> OnPostCancelAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            if (user.TenantID == null)
                return Forbid();

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .FirstOrDefaultAsync(m =>
                    m.MaintenanceRequestID == id.Value &&
                    m.TenantID == user.TenantID.Value);

            if (request == null)
                return NotFound();

            // Tenant can cancel only while request is still pending
            if (!string.Equals(
                request.Status,
                "Pending",
                StringComparison.OrdinalIgnoreCase))
            {
                TempData["ErrorMessage"] =
                    "Only pending maintenance requests can be cancelled.";

                return RedirectToPage("./Index");
            }

            request.Status = "Cancelled";
            request.CompletedDate = null;

            var tenantName = request.Tenant == null
                ? "A tenant"
                : $"{request.Tenant.FirstName} {request.Tenant.LastName}";

            var unitNumber = request.Unit?.UnitNumber ?? "N/A";

            await _notificationService.NotifyAdminsAsync(
                "Maintenance Request Cancelled",
                $"{tenantName} cancelled maintenance request " +
                $"#{request.MaintenanceRequestID} for Unit {unitNumber}: " +
                $"{request.RequestTitle}.",
                "Maintenance");

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Maintenance request cancelled successfully.";

            return RedirectToPage("./Index");
        }
    }
}