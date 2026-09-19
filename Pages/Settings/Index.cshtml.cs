using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Settings
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public SystemSetting SystemSetting { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSetting
                {
                    PropertyName = "Tressing Residency",
                    PropertyAddress = "",
                    ContactNumber = "",
                    Email = "",
                    Description = "",
                    DateUpdated = DateTime.Now
                };

                _context.SystemSettings.Add(settings);
                await _context.SaveChangesAsync();
            }

            SystemSetting = settings;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var settings = await _context.SystemSettings
                .FirstOrDefaultAsync();

            if (settings == null)
            {
                settings = new SystemSetting();

                _context.SystemSettings.Add(settings);
            }

            settings.PropertyName = SystemSetting.PropertyName.Trim();
            settings.PropertyAddress = SystemSetting.PropertyAddress.Trim();
            settings.ContactNumber = string.IsNullOrWhiteSpace(SystemSetting.ContactNumber)
                ? null
                : SystemSetting.ContactNumber.Trim();

            settings.Email = string.IsNullOrWhiteSpace(SystemSetting.Email)
                ? null
                : SystemSetting.Email.Trim();

            settings.Description = string.IsNullOrWhiteSpace(SystemSetting.Description)
                ? null
                : SystemSetting.Description.Trim();

            settings.DateUpdated = DateTime.Now;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "System settings updated successfully.";

            return RedirectToPage();
        }
    }
}