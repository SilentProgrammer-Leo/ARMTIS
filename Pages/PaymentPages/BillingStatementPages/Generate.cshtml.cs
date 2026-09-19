using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.MVVM.ViewModels;
using ARMTIS_Capstone_Project.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.BillingStatementPages
{
    public class GenerateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;

        public GenerateModel(
            ApplicationDbContext context,
            NotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        [BindProperty(SupportsGet = true)]
        public int? SelectedMonth { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? SelectedYear { get; set; }

        [BindProperty]
        public List<BillingGeneration> Bills { get; set; } = new();

        [TempData]
        public string? SuccessMessage { get; set; }

        public async Task OnGetAsync()
        {
            Bills = new List<BillingGeneration>();

            if (!SelectedMonth.HasValue ||
                !SelectedYear.HasValue)
            {
                return;
            }

            var selectedBillingMonth =
                new DateTime(
                    SelectedYear.Value,
                    SelectedMonth.Value,
                    1);

            var endOfSelectedMonth =
            selectedBillingMonth
                .AddMonths(1)
                .AddDays(-1);

            var tenants = await _context.Tenants
                 .Include(t => t.Unit)
                 .Where(t =>
                     t.LeaseStartDate <= endOfSelectedMonth &&
                     t.LeaseEndDate >= selectedBillingMonth)
                 .ToListAsync();

            foreach (var tenant in tenants)
            {
                if (tenant.Unit == null)
                    continue;

                // Check if this tenant has utility readings for the selected billing month.
                var utilityReading =
                    await _context.UtilityReadings
                        .FirstOrDefaultAsync(u =>
                            u.TenantID == tenant.TenantID &&
                            u.BillingMonth == selectedBillingMonth);

                // Check if billing was already generated for this tenant/month.
                var existingBilling =
                    await _context.BillingStatements
                        .FirstOrDefaultAsync(b =>
                            b.TenantID == tenant.TenantID &&
                            b.BillingMonth.Year ==
                                SelectedYear.Value &&
                            b.BillingMonth.Month ==
                                SelectedMonth.Value);

                var dueDate = selectedBillingMonth.AddMonths(1);

                Bills.Add(new BillingGeneration
                {
                    TenantID = tenant.TenantID,
                    UnitID = tenant.UnitID,

                    TenantName =
                        tenant.FirstName + " " +
                        tenant.LastName,

                    UnitNumber =
                        tenant.Unit.UnitNumber,

                    BillingMonth =
                        selectedBillingMonth,

                    DueDate =
                        dueDate,

                    MonthlyRent =
                        tenant.Unit.MonthlyRent,

                    Electricity =
                        utilityReading?.ElectricityBill ?? 0,

                    Water =
                        utilityReading?.WaterBill ?? 0,

                    Internet =
                        utilityReading?.InternetBill ?? 0,

                    UtilityTotal =
                        utilityReading?.TotalUtilityCharges ?? 0,

                    HasUtilityReading =
                        utilityReading != null,

                    BillingAlreadyExists =
                        existingBilling != null
                });
            }
        }

        public async Task<IActionResult> OnPostGenerateAllAsync(
    int selectedMonth,
    int selectedYear)
        {
            SelectedMonth = selectedMonth;
            SelectedYear = selectedYear;

            if (selectedMonth < 1 || selectedMonth > 12)
            {
                TempData["ErrorMessage"] =
                    "Please select a valid billing month.";

                return RedirectToPage(new
                {
                    SelectedMonth = selectedMonth,
                    SelectedYear = selectedYear
                });
            }

            var billingMonth =
                new DateTime(
                    selectedYear,
                    selectedMonth,
                    1);

            var endOfSelectedMonth =
                billingMonth
                    .AddMonths(1)
                    .AddDays(-1);

            // Load tenants who are still under an active lease
            // during the selected billing month.
            var tenants =
                await _context.Tenants
                    .Include(t => t.Unit)
                    .Where(t =>
                        t.LeaseStartDate <= endOfSelectedMonth &&
                        t.LeaseEndDate >= billingMonth)
                    .ToListAsync();

            if (tenants.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "No active tenants were found for the selected billing period.";

                return RedirectToPage(new
                {
                    SelectedMonth = selectedMonth,
                    SelectedYear = selectedYear
                });
            }

            int generatedCount = 0;
            int missingUtilityCount = 0;
            int duplicateCount = 0;
            int creditAppliedCount = 0;

            foreach (var tenant in tenants)
            {
                if (tenant.Unit == null)
                    continue;

                // FIND UTILITY READING

                var utility =
                    await _context.UtilityReadings
                        .FirstOrDefaultAsync(u =>
                            u.TenantID == tenant.TenantID &&
                            u.BillingMonth == billingMonth);

                // Do not generate incomplete billing.
                if (utility == null)
                {
                    missingUtilityCount++;
                    continue;
                }

                // CHECK DUPLICATE BILLING

                bool alreadyExists =
                    await _context.BillingStatements
                        .AnyAsync(b =>
                            b.TenantID == tenant.TenantID &&
                            b.BillingMonth.Year == selectedYear &&
                            b.BillingMonth.Month == selectedMonth);

                if (alreadyExists)
                {
                    duplicateCount++;
                    continue;
                }

                // GENERATE BILLING NUMBER

                string billingNo =
                    await GenerateBillingNumberAsync();

                // CALCULATE NORMAL BILL

                decimal utilityTotal =
                    utility.TotalUtilityCharges;

                decimal normalTotalAmountDue =
                    tenant.Unit.MonthlyRent +
                    utilityTotal;

                // APPLY TENANT CREDIT

                decimal creditApplied = 0;

                var availableCredits =
                    await _context.TenantCredits
                        .Where(c =>
                            c.TenantID == tenant.TenantID &&
                            c.RemainingAmount > 0)
                        .OrderBy(c => c.DateCreated)
                        .ToListAsync();

                decimal remainingBill =
                    normalTotalAmountDue;

                foreach (var credit in availableCredits)
                {
                    if (remainingBill <= 0)
                        break;

                    decimal amountToApply =
                        Math.Min(
                            credit.RemainingAmount,
                            remainingBill);

                    credit.RemainingAmount -= amountToApply;

                    creditApplied += amountToApply;
                    remainingBill -= amountToApply;
                }

                decimal totalAmountDue =
                    normalTotalAmountDue -
                    creditApplied;

                if (totalAmountDue < 0)
                    totalAmountDue = 0;

                if (creditApplied > 0)
                {
                    creditAppliedCount++;
                }

           
                // DETERMINE BILLING STATUS
               

                string billingStatus =
                    totalAmountDue <= 0
                        ? "Paid"
                        : "Pending";

                // CREATE BILLING STATEMENT
            

                var billingStatement =
                    new BillingStatement
                    {
                        TenantID =
                            tenant.TenantID,

                        UnitID =
                            tenant.UnitID,

                        BillingNo =
                            billingNo,

                        BillingMonth =
                            billingMonth,

                        // Based on your lease agreement,
                        // due date is the first day of the following month.
                        DueDate =
                            billingMonth.AddMonths(1),

                        MonthlyRent =
                            tenant.Unit.MonthlyRent,

                        Electricity =
                            utility.ElectricityBill,

                        Water =
                            utility.WaterBill,

                        Internet =
                            utility.InternetBill,

                        UtilityCharges =
                            utilityTotal,

                        PenaltyAmount =
                            0,

                        // Normal bill minus available tenant credit
                        TotalAmountDue =
                            totalAmountDue,

                        // Amount of tenant credit used
                        CreditApplied =
                            creditApplied,

                        BillingStatus =
                            billingStatus,

                        Remarks =
                            creditApplied > 0
                                ? $"Tenant credit of ₱{creditApplied:N2} applied."
                                : null,

                        DateGenerated =
                            DateTime.Today
                    };

                _context.BillingStatements.Add(
                    billingStatement);

                await _context.SaveChangesAsync();

                await _notificationService.NotifyTenantAsync(
                    tenant.TenantID,
                    "New Billing Statement",
                    $"Your billing statement for {billingMonth:MMMM yyyy} has been generated. Billing No.: {billingNo}. Please check your Bills page for the amount due and due date.",
                    "Billing");

                generatedCount++;
            }

            // RESULT MESSAGE

            SuccessMessage =
                $"{generatedCount} billing statement(s) generated. " +
                $"{creditAppliedCount} tenant credit(s) applied. " +
                $"{missingUtilityCount} missing utility reading(s). " +
                $"{duplicateCount} already generated.";

            return RedirectToPage("./Index");
        }

        private static DateTime GetBillingDate(
            DateTime leaseStartDate,
            int year,
            int month)
        {
            // Keeps the tenant's lease-start day.
            // Example: lease started on the 15th,
            // so every billing date is on the 15th.
            int day = Math.Min(
                leaseStartDate.Day,
                DateTime.DaysInMonth(year, month));

            return new DateTime(year, month, day);
        }

        private async Task<string> GenerateBillingNumberAsync()
        {
            int nextNumber =
                (await _context.BillingStatements
                    .MaxAsync(b => (int?)b.BillingStatementID) ?? 0) + 1;

            return $"BILL-{DateTime.Now:yyyy}-{nextNumber:D5}";
        }
    }
}
