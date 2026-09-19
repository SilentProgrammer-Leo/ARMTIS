using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Utilities
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

        public List<UtilityReading> UtilityReadings { get; set; }
            = new List<UtilityReading>();

        public decimal TotalElectricityConsumption { get; set; }

        public decimal TotalWaterConsumption { get; set; }

        public decimal TotalUtilityCharges { get; set; }

        public int TotalRecords { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the currently logged-in user
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Tenant account must be linked to a Tenant record
            if (user.TenantID == null)
            {
                return Forbid();
            }

            int tenantID = user.TenantID.Value;

            // Get ONLY utility records belonging to this tenant
            UtilityReadings = await _context.UtilityReadings
                .AsNoTracking()
                .Where(u => u.TenantID == tenantID)
                .OrderByDescending(u => u.BillingMonth)
                .ToListAsync();

            TotalRecords = UtilityReadings.Count;

            TotalElectricityConsumption =
                UtilityReadings.Sum(u => u.ElectricConsumption);

            TotalWaterConsumption =
                UtilityReadings.Sum(u => u.WaterConsumption);

            // Utility charges are stored in the BillingStatement,
            // so calculate them separately using the tenant's billing records.
            var billingMonths = UtilityReadings
                .Select(u => u.BillingMonth)
                .ToList();

            if (billingMonths.Count > 0)
            {
                TotalUtilityCharges = await _context.BillingStatements
                    .AsNoTracking()
                    .Where(b =>
                        b.TenantID == tenantID &&
                        billingMonths.Contains(b.BillingMonth))
                    .SumAsync(b => b.UtilityCharges);
            }

            return Page();
        }
    }
}