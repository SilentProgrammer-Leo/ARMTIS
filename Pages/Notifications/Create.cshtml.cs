using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Notifications
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CreateModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Notification Notification { get; set; } = new();

        public SelectList TenantList { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync()
        {
            await LoadTenantsAsync();

            Notification.CreatedDate = DateTime.Now;
            Notification.IsRead = false;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // These values are controlled by the system.
            Notification.CreatedDate = DateTime.Now;
            Notification.IsRead = false;

            // Navigation property should not be validated.
            ModelState.Remove("Notification.Tenant");

            if (Notification.TenantID == null)
            {
                ModelState.AddModelError(
                    "Notification.TenantID",
                    "Please select a tenant.");
            }

            if (!ModelState.IsValid)
            {
                await LoadTenantsAsync();
                return Page();
            }

            var tenantExists = await _context.Tenants
                .AnyAsync(t => t.TenantID == Notification.TenantID);

            if (!tenantExists)
            {
                ModelState.AddModelError(
                    "Notification.TenantID",
                    "The selected tenant does not exist.");

                await LoadTenantsAsync();
                return Page();
            }

            // Only save fields intended for notification creation.
            var newNotification = new Notification
            {
                TenantID = Notification.TenantID,
                Title = Notification.Title.Trim(),
                Message = Notification.Message.Trim(),
                NotificationType = string.IsNullOrWhiteSpace(
                    Notification.NotificationType)
                    ? null
                    : Notification.NotificationType.Trim(),
                IsRead = false,
                CreatedDate = DateTime.Now
            };

            _context.Notifications.Add(newNotification);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Notification created successfully.";

            return RedirectToPage("./Index");
        }

        private async Task LoadTenantsAsync()
        {
            var tenants = await _context.Tenants
                .AsNoTracking()
                .OrderBy(t => t.LastName)
                .ThenBy(t => t.FirstName)
                .Select(t => new
                {
                    t.TenantID,
                    FullName = t.FirstName + " " + t.LastName
                })
                .ToListAsync();

            TenantList = new SelectList(
                tenants,
                "TenantID",
                "FullName");
        }
    }
}