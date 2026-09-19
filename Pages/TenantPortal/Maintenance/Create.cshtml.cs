using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Services;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Maintenance
{
    [Authorize(Roles = "Tenant")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly NotificationService _notificationService;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            NotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        [BindProperty]
        public MaintenanceRequest MaintenanceRequest { get; set; }
            = new MaintenanceRequest();

        public string UnitNumber { get; set; } = "N/A";

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            if (user.TenantID == null)
                return Forbid();

            var tenant = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.Unit)
                .FirstOrDefaultAsync(t => t.TenantID == user.TenantID.Value);

            if (tenant == null)
                return NotFound();

            UnitNumber = tenant.Unit?.UnitNumber ?? "N/A";

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            if (user.TenantID == null)
                return Forbid();

            var tenant = await _context.Tenants
                .AsNoTracking()
                .Include(t => t.Unit)
                .FirstOrDefaultAsync(t => t.TenantID == user.TenantID.Value);

            if (tenant == null)
                return NotFound();

            UnitNumber = tenant.Unit?.UnitNumber ?? "N/A";

            // Use the logged-in tenant's IDs
            MaintenanceRequest.TenantID = tenant.TenantID;
            MaintenanceRequest.UnitID = tenant.UnitID;

            // System-controlled values
            MaintenanceRequest.Status = "Pending";
            MaintenanceRequest.RequestDate = DateTime.Now;
            MaintenanceRequest.CompletedDate = null;
            MaintenanceRequest.AdminRemarks = null;

            if (string.IsNullOrWhiteSpace(MaintenanceRequest.RequestTitle))
            {
                ModelState.AddModelError(
                    "MaintenanceRequest.RequestTitle",
                    "Please enter a request title.");
            }

            if (string.IsNullOrWhiteSpace(MaintenanceRequest.Description))
            {
                ModelState.AddModelError(
                    "MaintenanceRequest.Description",
                    "Please describe the maintenance problem.");
            }

            if (!ModelState.IsValid)
                return Page();

            _context.MaintenanceRequests.Add(MaintenanceRequest);

            await _context.SaveChangesAsync();

            await _notificationService.NotifyAdminsAsync(
                "New Maintenance Request",
                $"{tenant.FirstName} {tenant.LastName} submitted a maintenance request for Unit {tenant.Unit?.UnitNumber ?? "N/A"}: {MaintenanceRequest.RequestTitle}.",
                "Maintenance");

            TempData["SuccessMessage"] =
                "Maintenance request submitted successfully.";

            return RedirectToPage("./Index");
        }
    }
}