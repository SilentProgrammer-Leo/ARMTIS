using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Notifications
{
    [Authorize(Roles = "Admin")]
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

        public IList<Notification> Notifications { get; set; }
            = new List<Notification>();

        public int UnreadCount { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            Notifications = await _context.Notifications
                .Include(n => n.Tenant)
                .Where(n => n.RecipientUserID == currentUser.Id)
                .AsNoTracking()
                .OrderByDescending(n => n.CreatedDate)
                .ToListAsync();

            UnreadCount = Notifications.Count(n => !n.IsRead);

            return Page();
        }

        public async Task<IActionResult> OnPostMarkAsReadAsync(int id)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            var notification = await _context.Notifications
                .FirstOrDefaultAsync(n =>
                    n.NotificationID == id &&
                    n.RecipientUserID == currentUser.Id);

            if (notification == null)
                return NotFound();

            notification.IsRead = true;
            await _context.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostMarkAllAsReadAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if (currentUser == null)
                return RedirectToPage("/Account/Login");

            var unread = await _context.Notifications
                .Where(n =>
                    n.RecipientUserID == currentUser.Id &&
                    !n.IsRead)
                .ToListAsync();

            foreach (var notification in unread)
                notification.IsRead = true;

            if (unread.Count > 0)
                await _context.SaveChangesAsync();

            return RedirectToPage();
        }
    }
}
