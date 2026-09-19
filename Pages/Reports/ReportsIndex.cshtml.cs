using ARMTIS_Capstone_Project.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Reports
{
    [Authorize(Roles = "Admin")]
    public class ReportsIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReportsIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==========================================
        // INCOME REPORT FILTER
        // ==========================================

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        // ==========================================
        // MONTHLY INCOME
        // ==========================================

        public decimal TotalPaymentsCollected { get; set; }

        public int PaymentCount { get; set; }

        public class PaymentSummary
        {
            public DateTime PaymentDate { get; set; }
            public string TenantName { get; set; } = string.Empty;
            public string PaymentMethod { get; set; } = string.Empty;
            public string ReceiptNo { get; set; } = string.Empty;
            public decimal AmountPaid { get; set; }
        }

        public IList<PaymentSummary> PaymentSummaries { get; set; }
            = new List<PaymentSummary>();

        // ==========================================
        // OCCUPANCY
        // ==========================================

        public int TotalUnits { get; set; }

        public int OccupiedUnits { get; set; }

        public int VacantUnits { get; set; }

        public decimal OccupancyRate { get; set; }

        public class UnitOccupancySummary
        {
            public string UnitNumber { get; set; } = string.Empty;
            public string UnitStatus { get; set; } = string.Empty;
            public string TenantName { get; set; } = "—";
        }

        public IList<UnitOccupancySummary> OccupancyUnits { get; set; }
            = new List<UnitOccupancySummary>();

        // ==========================================
        // UNPAID TENANT BALANCES
        // ==========================================

        public decimal TotalOutstandingBalance { get; set; }

        public class UnpaidTenantSummary
        {
            public string TenantName { get; set; } = string.Empty;
            public string UnitNumber { get; set; } = "—";
            public string BillingNo { get; set; } = string.Empty;
            public DateTime BillingMonth { get; set; }
            public DateTime DueDate { get; set; }
            public decimal TotalAmountDue { get; set; }
            public decimal PaidAmount { get; set; }
            public decimal RemainingBalance { get; set; }
            public string BillingStatus { get; set; } = string.Empty;
        }

        public IList<UnpaidTenantSummary> UnpaidTenantBalances { get; set; }
            = new List<UnpaidTenantSummary>();

        // ==========================================
        // LOAD REPORTS
        // ==========================================

        public async Task OnGetAsync()
        {
            var today = DateTime.Today;

            // Default to the current month.
            if (StartDate == default)
            {
                StartDate = new DateTime(
                    today.Year,
                    today.Month,
                    1);
            }

            if (EndDate == default)
            {
                EndDate = today;
            }

            if (EndDate < StartDate)
            {
                EndDate = StartDate;
            }

            var endDateExclusive = EndDate.Date.AddDays(1);

            // ==========================================
            // 1. MONTHLY INCOME REPORT
            // ==========================================

            var paymentQuery = _context.RentalPayments
                .Include(p => p.Tenant)
                .Where(p =>
                    p.PaymentDate >= StartDate.Date &&
                    p.PaymentDate < endDateExclusive);

            TotalPaymentsCollected =
                await paymentQuery
                    .SumAsync(p => (decimal?)p.AmountPaid) ?? 0;

            PaymentSummaries =
                await paymentQuery
                    .AsNoTracking()
                    .OrderByDescending(p => p.PaymentDate)
                    .Select(p => new PaymentSummary
                    {
                        PaymentDate = p.PaymentDate,

                        TenantName =
                            p.Tenant != null
                                ? p.Tenant.FirstName + " " + p.Tenant.LastName
                                : "N/A",

                        PaymentMethod =
                            p.PaymentMethod,

                        ReceiptNo =
                            p.ReceiptNo,

                        AmountPaid =
                            p.AmountPaid
                    })
                    .ToListAsync();

            PaymentCount = PaymentSummaries.Count;

            // ==========================================
            // 2. OCCUPANCY REPORT
            // ==========================================

            var units =
                await _context.Units
                    .Include(u => u.Tenants)
                    .AsNoTracking()
                    .OrderBy(u => u.UnitNumber)
                    .ToListAsync();

            TotalUnits = units.Count;

            OccupiedUnits =
                units.Count(u =>
                    u.UnitStatus == "Occupied");

            VacantUnits =
                units.Count(u =>
                    u.UnitStatus == "Vacant");

            OccupancyRate =
                TotalUnits == 0
                    ? 0
                    : (decimal)OccupiedUnits /
                      TotalUnits *
                      100;

            OccupancyUnits =
                units.Select(u => new UnitOccupancySummary
                {
                    UnitNumber = u.UnitNumber,
                    UnitStatus = u.UnitStatus,

                    TenantName =
                        u.Tenants != null && u.Tenants.Any()
                            ? string.Join(
                                ", ",
                                u.Tenants.Select(t =>
                                    t.FirstName + " " + t.LastName))
                            : "—"
                }).ToList();

            // ==========================================
            // 3. UNPAID TENANT BALANCES
            // ==========================================

            var unpaidBills =
                await _context.BillingStatements
                    .Include(b => b.Tenant)
                    .Include(b => b.Unit)
                    .AsNoTracking()
                    .Where(b =>
                        b.BillingStatus != "Paid")
                    .OrderBy(b => b.DueDate)
                    .ToListAsync();

            var unpaidBillingIds =
                unpaidBills
                    .Select(b => b.BillingStatementID)
                    .ToList();

            var appliedPaymentsByBill =
                await _context.RentalPayments
                    .Where(p =>
                        p.BillingStatementID.HasValue &&
                        unpaidBillingIds.Contains(
                            p.BillingStatementID.Value))
                    .GroupBy(p =>
                        p.BillingStatementID!.Value)
                    .Select(g => new
                    {
                        BillingStatementID = g.Key,
                        PaidAmount = g.Sum(p => p.AppliedAmount)
                    })
                    .ToDictionaryAsync(
                        x => x.BillingStatementID,
                        x => x.PaidAmount);

            foreach (var bill in unpaidBills)
            {
                decimal paidAmount =
                    appliedPaymentsByBill.TryGetValue(
                        bill.BillingStatementID,
                        out var paid)
                        ? paid
                        : 0;

                decimal remainingBalance =
                    bill.TotalAmountDue -
                    paidAmount -
                    bill.CreditApplied;

                if (remainingBalance <= 0)
                {
                    continue;
                }

                UnpaidTenantBalances.Add(
                    new UnpaidTenantSummary
                    {
                        TenantName =
                            bill.Tenant != null
                                ? bill.Tenant.FirstName +
                                  " " +
                                  bill.Tenant.LastName
                                : "N/A",

                        UnitNumber =
                            bill.Unit?.UnitNumber ?? "—",

                        BillingNo =
                            bill.BillingNo,

                        BillingMonth =
                            bill.BillingMonth,

                        DueDate =
                            bill.DueDate,

                        TotalAmountDue =
                            bill.TotalAmountDue,

                        PaidAmount =
                            paidAmount + bill.CreditApplied,

                        RemainingBalance =
                            remainingBalance,

                        BillingStatus =
                            bill.BillingStatus
                    });
            }

            TotalOutstandingBalance =
                UnpaidTenantBalances.Sum(
                    x => x.RemainingBalance);
        }
    }
}