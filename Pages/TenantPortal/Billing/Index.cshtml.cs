using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Billing
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

        public List<BillingStatement> BillingStatements { get; set; }
            = new List<BillingStatement>();

        public Dictionary<int, decimal> PaidAmounts { get; set; }
    = new Dictionary<int, decimal>();

        public decimal TotalOutstanding { get; set; }

        public decimal TotalPaid { get; set; }

        public int UnpaidBills { get; set; }

        public int PaidBills { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return RedirectToPage("/Account/Login");

            if (user.TenantID == null)
                return Forbid();

            int tenantID = user.TenantID.Value;

            // Get this tenant's billing statements
            BillingStatements = await _context.BillingStatements
                .AsNoTracking()
                .Include(b => b.Unit)
                .Where(b => b.TenantID == tenantID)
                .OrderByDescending(b => b.BillingMonth)
                .ThenByDescending(b => b.DateGenerated)
                .ToListAsync();

            // Get the billing statement IDs
            var billingIds = BillingStatements
                .Select(b => b.BillingStatementID)
                .ToList();

            // Get payments applied to these bills
            if (billingIds.Count > 0)
            {
                var paymentsByBilling = await _context.RentalPayments
                    .Where(p => p.BillingStatementID.HasValue &&
                                billingIds.Contains(p.BillingStatementID.Value))
                    .GroupBy(p => p.BillingStatementID!.Value)
                    .Select(g => new
                    {
                        BillingStatementID = g.Key,
                        PaidAmount = g.Sum(p => p.AppliedAmount)
                    })
                    .ToDictionaryAsync(
                        x => x.BillingStatementID,
                        x => x.PaidAmount);

                PaidAmounts = paymentsByBilling;
            }

            // Calculate summary
            TotalOutstanding = BillingStatements.Sum(b =>
            {
                decimal paid = PaidAmounts.GetValueOrDefault(
                    b.BillingStatementID, 0);

                decimal balance = b.TotalAmountDue - paid;

                return balance > 0 ? balance : 0;
            });

            UnpaidBills = BillingStatements.Count(b =>
            {
                decimal paid = PaidAmounts.GetValueOrDefault(
                    b.BillingStatementID, 0);

                decimal balance = b.TotalAmountDue - paid;

                return balance > 0;
            });

            PaidBills = BillingStatements.Count(b =>
            {
                decimal paid = PaidAmounts.GetValueOrDefault(
                    b.BillingStatementID, 0);

                decimal balance = b.TotalAmountDue - paid;

                return balance <= 0;
            });

            return Page();
        }
    }
}