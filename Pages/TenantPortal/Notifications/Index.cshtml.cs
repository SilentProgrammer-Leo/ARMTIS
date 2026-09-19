using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Notifications
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

        public IList<Notification> Notifications { get; set; } = new List<Notification>();

        public int UnreadCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            if (user.TenantID == null)
                return Forbid();

            int tenantID = user.TenantID.Value;

            Notifications = await _context.Notifications
                .Where(n => n.TenantID == tenantID)
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            UnreadCount = Notifications.Count(n => !n.IsRead);

            return Page();
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            if (user.TenantID == null)
                return Forbid();

            int tenantID = user.TenantID.Value;

            // IMPORTANT:
            // The notification must belong to the currently logged-in tenant.
            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationID == id &&
                    n.TenantID == tenantID);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;

            await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}