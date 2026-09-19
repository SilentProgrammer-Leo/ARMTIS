using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Services;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages
{
    public class UtilityRateCreateIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public UtilityRateCreateIndexModel(
            ApplicationDbContext context,
            NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [BindProperty]
        public UtilityRate UtilityRate { get; set; }
          = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (UtilityRate.Rate <= 0)
            {
                ModelState.AddModelError(
                    "UtilityRate.Rate",
                    "Rate must be greater than zero."
                );

                return Page();
            }

            var currentRate =
                await _context.UtilityRates
                    .FirstOrDefaultAsync(r =>
                        r.UtilityType ==
                            UtilityRate.UtilityType &&
                        r.IsActive);

            if (currentRate != null)
            {
                if (UtilityRate.EffectiveFrom <=
                    currentRate.EffectiveFrom)
                {
                    ModelState.AddModelError(
                        "UtilityRate.EffectiveFrom",
                        "The new rate must take effect after the current rate."
                    );

                    return Page();
                }

                currentRate.EffectiveTo =
                    UtilityRate.EffectiveFrom
                        .AddDays(-1);

                currentRate.IsActive = false;
            }

            UtilityRate.EffectiveTo = null;

            UtilityRate.IsActive = true;

            UtilityRate.DateCreated =
                DateTime.Now;

            _context.UtilityRates.Add(
                UtilityRate);

            await _context.SaveChangesAsync();

            await _notificationService.NotifyAllTenantsAsync(
                "Utility Rate Updated",
                $"The {UtilityRate.UtilityType} utility rate has been updated to ₱{UtilityRate.Rate:N2}. The new rate will apply beginning {UtilityRate.EffectiveFrom:MMMM dd, yyyy}.",
                "Utility");

            return RedirectToPage(
                "./UtilityRateIndex"
            );
        }
    }
}
