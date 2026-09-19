
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // ==============================
        // UNIT / TENANT SUMMARY
        // ==============================

        public int TotalUnits { get; set; }

        public int ActiveTenants { get; set; }

        public int OccupiedUnits { get; set; }

        public int VacantUnits { get; set; }


        // ==============================
        // FINANCIAL SUMMARY
        // ==============================

        public decimal MonthlyCollections { get; set; }

        public decimal OutstandingBalance { get; set; }

        public int PaidBills { get; set; }

        public int UnpaidBills { get; set; }

        public int OverdueBills { get; set; }


        // ==============================
        // MAINTENANCE SUMMARY
        // ==============================

        public int PendingMaintenance { get; set; }

        public int InProgressMaintenance { get; set; }

        public int CompletedMaintenance { get; set; }


        // ==============================
        // NOTIFICATION SUMMARY
        // ==============================

        public int TotalNotifications { get; set; }

        public int UnreadNotifications { get; set; }


        // ==============================
        // TABLE DATA
        // ==============================

        public IList<BillingStatement> CurrentMonthBillings { get; set; }
            = new List<BillingStatement>();

        public IList<RentalPayment> RecentPayments { get; set; }
            = new List<RentalPayment>();

        public IList<MaintenanceRequest> RecentMaintenanceRequests { get; set; }
            = new List<MaintenanceRequest>();


        // ==============================
        // UPDATE BILLING PENALTY / STATUS
        // ==============================

        private void UpdatePenaltyAndStatus(BillingStatement billing)
        {
            // PAID should never change
            if (billing.BillingStatus == "Paid")
            {
                return;
            }

            decimal baseAmount =
                billing.MonthlyRent +
                billing.UtilityCharges;

            var gracePeriodEnd =
                billing.DueDate.AddDays(7);


            // BEFORE / WITHIN GRACE PERIOD
            if (DateTime.Today <= gracePeriodEnd)
            {
                billing.PenaltyAmount = 0;

                billing.TotalAmountDue =
                    baseAmount;

                // Preserve Unpaid
                if (billing.BillingStatus != "Unpaid")
                {
                    billing.BillingStatus =
                        "Pending";
                }

                return;
            }


            // AFTER GRACE PERIOD
            int lateDays =
                (DateTime.Today - gracePeriodEnd).Days;

            decimal dailyPenalty =
                baseAmount * 0.04m;

            billing.PenaltyAmount =
                dailyPenalty * lateDays;

            billing.TotalAmountDue =
                baseAmount +
                billing.PenaltyAmount;


            if (billing.BillingStatus == "Unpaid" ||
                billing.BillingStatus == "Overdue")
            {
                billing.BillingStatus =
                    "Overdue";
            }
        }


        // ==============================
        // LOAD DASHBOARD
        // ==============================

        public async Task OnGetAsync()
        {
            // ==============================
            // UNIT SUMMARY
            // ==============================

            TotalUnits =
                await _context.Units
                    .CountAsync();


            OccupiedUnits =
                await _context.Units
                    .CountAsync(u =>
                        u.UnitStatus == "Occupied");


            VacantUnits =
                await _context.Units
                    .CountAsync(u =>
                        u.UnitStatus == "Vacant");


            // ==============================
            // TENANT SUMMARY
            // ==============================

            ActiveTenants =
                await _context.Tenants
                    .CountAsync(t =>
                        t.Status == "Active");


            // ==============================
            // DATE RANGE
            // ==============================

            var today = DateTime.Today;

            var startOfMonth =
                new DateTime(
                    today.Year,
                    today.Month,
                    1);

            var startOfNextMonth =
                startOfMonth.AddMonths(1);


            // ==============================
            // MONTHLY COLLECTIONS
            // ==============================

            MonthlyCollections =
                await _context.RentalPayments
                    .Where(p =>
                        p.PaymentDate >= startOfMonth &&
                        p.PaymentDate < startOfNextMonth)
                    .SumAsync(p =>
                        (decimal?)p.AmountPaid) ?? 0;


            // ==============================
            // CURRENT MONTH BILLINGS
            // ==============================

            CurrentMonthBillings =
                await _context.BillingStatements
                    .Include(b => b.Tenant)
                    .Include(b => b.Unit)
                    .Where(b =>
                        b.BillingMonth.Year == today.Year &&
                        b.BillingMonth.Month == today.Month)
                    .OrderBy(b => b.DueDate)
                    .ToListAsync();


            // Update penalty and billing status
            foreach (var billing in CurrentMonthBillings)
            {
                UpdatePenaltyAndStatus(billing);
            }


            // ==============================
            // SAVE BILLING UPDATES
            // ==============================

            await _context.SaveChangesAsync();


            // ==============================
            // OUTSTANDING BALANCE
            // ==============================

            OutstandingBalance =
                await _context.BillingStatements
                    .Where(b =>
                        b.BillingStatus != "Paid")
                    .SumAsync(b =>
                        (decimal?)b.TotalAmountDue) ?? 0;


            // ==============================
            // BILLING STATUS COUNTS
            // ==============================

            PaidBills =
                await _context.BillingStatements
                    .CountAsync(b =>
                        b.BillingStatus == "Paid");


            UnpaidBills =
                await _context.BillingStatements
                    .CountAsync(b =>
                        b.BillingStatus == "Unpaid");


            OverdueBills =
                await _context.BillingStatements
                    .CountAsync(b =>
                        b.BillingStatus == "Overdue");


            // ==============================
            // MAINTENANCE SUMMARY
            // ==============================

            PendingMaintenance =
                await _context.MaintenanceRequests
                    .CountAsync(m =>
                        m.Status == "Pending");


            InProgressMaintenance =
                await _context.MaintenanceRequests
                    .CountAsync(m =>
                        m.Status == "In Progress");


            CompletedMaintenance =
                await _context.MaintenanceRequests
                    .CountAsync(m =>
                        m.Status == "Completed");


            // ==============================
            // NOTIFICATION SUMMARY
            // ==============================

            TotalNotifications =
                await _context.Notifications
                    .CountAsync();


            UnreadNotifications =
                await _context.Notifications
                    .CountAsync(n =>
                        !n.IsRead);


            // ==============================
            // RECENT PAYMENTS
            // ==============================

            RecentPayments =
                await _context.RentalPayments
                    .Include(p => p.Tenant)
                    .AsNoTracking()
                    .OrderByDescending(p => p.PaymentDate)
                    .Take(5)
                    .ToListAsync();


            // ==============================
            // RECENT MAINTENANCE REQUESTS
            // ==============================

            RecentMaintenanceRequests =
                await _context.MaintenanceRequests
                    .Include(m => m.Tenant)
                    .Include(m => m.Unit)
                    .AsNoTracking()
                    .OrderByDescending(m => m.RequestDate)
                    .Take(5)
                    .ToListAsync();
        }
    }
}

