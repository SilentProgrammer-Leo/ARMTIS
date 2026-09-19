using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Services;

namespace ARMTIS_Capstone_Project.Pages.Maintenance
{
    [Authorize(Roles = "Admin")]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public EditModel(
            ApplicationDbContext context,
            NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public MaintenanceRequest MaintenanceRequest { get; set; } = null!;

        [BindProperty]
        public string Priority { get; set; } = "Medium";

        [BindProperty]
        public string Status { get; set; } = "Pending";

        [BindProperty]
        public string? AdminRemarks { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var request = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.MaintenanceRequestID == id.Value);

            if (request == null)
                return NotFound();

            MaintenanceRequest = request;

            Priority = request.Priority;
            Status = request.Status;
            AdminRemarks = request.AdminRemarks;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
                return NotFound();

            var request = await _context.MaintenanceRequests
                .FirstOrDefaultAsync(m => m.MaintenanceRequestID == id.Value);

            if (request == null)
                return NotFound();

            string previousStatus = request.Status;

            string[] allowedPriorities =
            {
                "Low",
                "Medium",
                "High"
            };

            string[] allowedStatuses =
            {
                "Pending",
                "In Progress",
                "Completed"
            };

            if (!allowedPriorities.Contains(Priority))
            {
                ModelState.AddModelError(
                    nameof(Priority),
                    "Invalid priority selected.");
            }

            if (!allowedStatuses.Contains(Status))
            {
                ModelState.AddModelError(
                    nameof(Status),
                    "Invalid status selected.");
            }

            if (!ModelState.IsValid)
            {
                MaintenanceRequest = await _context.MaintenanceRequests
                    .Include(m => m.Tenant)
                    .Include(m => m.Unit)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        m => m.MaintenanceRequestID == id.Value);

                return Page();
            }

            request.Priority = Priority;
            request.Status = Status;

            request.AdminRemarks =
                string.IsNullOrWhiteSpace(AdminRemarks)
                    ? null
                    : AdminRemarks.Trim();

            if (Status == "Completed")
            {
                if (!request.CompletedDate.HasValue)
                {
                    request.CompletedDate = DateTime.Now;
                }
            }
            else
            {
                request.CompletedDate = null;
            }

            await _context.SaveChangesAsync();

            if (!string.Equals(previousStatus, request.Status, StringComparison.OrdinalIgnoreCase))
            {
                await _notificationService.NotifyTenantAsync(
                    request.TenantID,
                    "Maintenance Request Updated",
                    $"Your maintenance request \"{request.RequestTitle}\" is now marked as {request.Status}.",
                    "Maintenance");
            }

            TempData["SuccessMessage"] =
                "Maintenance request updated successfully.";

            return RedirectToPage(
                "./Details",
                new { id = request.MaintenanceRequestID });
        }
    }
}