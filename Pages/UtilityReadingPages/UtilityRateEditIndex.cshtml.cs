using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Services;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages
{
    public class UtilityRateEditIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public UtilityRateEditIndexModel(
            ApplicationDbContext context,
            NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [BindProperty]
        public UtilityRate UtilityRate { get; set; }
            = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var utilityRate =
                await _context.UtilityRates
                    .FirstOrDefaultAsync(r =>
                        r.UtilityRateID == id);

            if (utilityRate == null)
            {
                return NotFound();
            }

            UtilityRate = utilityRate;

            return Page();
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

            var existingRate =
                await _context.UtilityRates
                    .FirstOrDefaultAsync(r =>
                        r.UtilityRateID ==
                        UtilityRate.UtilityRateID);

            if (existingRate == null)
            {
                return NotFound();
            }

            bool rateChanged =
                existingRate.UtilityType != UtilityRate.UtilityType ||
                existingRate.Rate != UtilityRate.Rate ||
                existingRate.EffectiveFrom != UtilityRate.EffectiveFrom ||
                existingRate.EffectiveTo != UtilityRate.EffectiveTo;

            existingRate.UtilityType =
                UtilityRate.UtilityType;

            existingRate.Rate =
                UtilityRate.Rate;

            existingRate.EffectiveFrom =
                UtilityRate.EffectiveFrom;

            existingRate.EffectiveTo =
                UtilityRate.EffectiveTo;

            await _context.SaveChangesAsync();

            if (rateChanged)
            {
                await _notificationService.NotifyAllTenantsAsync(
                    "Utility Rate Updated",
                    $"The {existingRate.UtilityType} utility rate has been updated to ₱{existingRate.Rate:N2}. Please check your utility and billing information.",
                    "Utility");
            }

            TempData["SuccessMessage"] =
                "Utility rate updated successfully.";

            return RedirectToPage(
                "./UtilityRateIndex"
            );
        }
    }
}
