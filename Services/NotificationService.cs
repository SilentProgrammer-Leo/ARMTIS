using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationService(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task NotifyTenantAsync(
            int tenantId,
            string title,
            string message,
            string? notificationType = null)
        {
            _context.Notifications.Add(new Notification
            {
                TenantID = tenantId,
                RecipientUserID = null,
                Title = title,
                Message = message,
                NotificationType = notificationType,
                IsRead = false,
                CreatedDate = DateTime.Now
            });

            await _context.SaveChangesAsync();
        }

        public async Task NotifyAllTenantsAsync(
            string title,
            string message,
            string? notificationType = null)
        {
            var tenants = await _context.Tenants
                .Where(t => t.Status == "Active")
                .AsNoTracking()
                .ToListAsync();

            foreach (var tenant in tenants)
            {
                _context.Notifications.Add(new Notification
                {
                    TenantID = tenant.TenantID,
                    RecipientUserID = null,
                    Title = title,
                    Message = message,
                    NotificationType = notificationType,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }

        public async Task NotifyAdminsAsync(
            string title,
            string message,
            string? notificationType = null)
        {
            var adminUsers = await _userManager
                .GetUsersInRoleAsync("Admin");

            foreach (var admin in adminUsers)
            {
                _context.Notifications.Add(new Notification
                {
                    TenantID = null,
                    RecipientUserID = admin.Id,
                    Title = title,
                    Message = message,
                    NotificationType = notificationType,
                    IsRead = false,
                    CreatedDate = DateTime.Now
                });
            }

            await _context.SaveChangesAsync();
        }
    }
}
