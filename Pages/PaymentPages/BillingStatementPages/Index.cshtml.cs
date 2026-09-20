using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;


namespace ARMTIS_Capstone_Project.Pages.BillingStatementPages;

public class IndexModel : PageModel
{
    [BindProperty(SupportsGet = true)]
    public string? SearchTerm { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? StatusFilter { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? BillingMonthFilter { get; set; }
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Dictionary<int, UtilityReading> UtilityReadingsByBillingId { get; set; }
    = new();

    private void UpdatePenaltyAndStatus(
    BillingStatement billing)
    {
        // PAID should never be changed.
        if (billing.BillingStatus == "Paid")
        {
            return;
        }

        decimal baseAmount =billing.MonthlyRent + billing.UtilityCharges;

        var gracePeriodEnd =billing.DueDate.AddDays(7);

        // BEFORE / WITHIN GRACE PERIOD

        if (DateTime.Today <= gracePeriodEnd)
        {
            billing.PenaltyAmount = 0;

            billing.TotalAmountDue = baseAmount;

            // To not change Unpaid back to Pending.
            if (billing.BillingStatus != "Unpaid")
            {
                billing.BillingStatus = "Pending";
            }

            return;
        }

        // AFTER GRACE PERIOD

        int lateDays =(DateTime.Today -gracePeriodEnd).Days;

        decimal dailyPenalty = baseAmount * 0.04m;

        billing.PenaltyAmount = dailyPenalty * lateDays;

        billing.TotalAmountDue = baseAmount + billing.PenaltyAmount;

        // Only issued/unpaid bills become overdue.
        if (billing.BillingStatus == "Unpaid" || billing.BillingStatus == "Overdue")
        {
            billing.BillingStatus =
                "Overdue";
        }
    }   

    public IList<BillingStatement> BillingStatement { get; set; }
    = new List<BillingStatement>();

    public async Task OnGetAsync()
    {
  
        // LOAD ALL BILLING STATEMENTS

        var allBillingStatements = await _context.BillingStatements
            .Include(b => b.Tenant)
            .Include(b => b.Unit)
            .OrderByDescending(b => b.DateGenerated)
            .ToListAsync();

        // INITIALIZE UTILITY READING DICTIONARY

        UtilityReadingsByBillingId = new Dictionary<int, UtilityReading>();

        // UPDATE PENALTY / STATUS
        // AND LOAD UTILITY READINGS

        foreach (var billing in allBillingStatements)
        {
            UpdatePenaltyAndStatus(billing);

            var billingMonth = new DateTime(billing.BillingMonth.Year,billing.BillingMonth.Month,1);

            var utilityReading = await _context.UtilityReadings
                    .FirstOrDefaultAsync(
                        u =>u.TenantID == billing.TenantID && u.BillingMonth == billingMonth);

            if (utilityReading != null)
            {
                UtilityReadingsByBillingId[billing.BillingStatementID] = utilityReading;
            }
        }


        // SAVE UPDATED PENALTIES / STATUS

        await _context.SaveChangesAsync();

        // APPLY SEARCH

        IEnumerable<BillingStatement> filteredBillingStatements =
            allBillingStatements;

        if (!string.IsNullOrWhiteSpace(SearchTerm))
        {
            var search = SearchTerm.Trim();

            filteredBillingStatements = filteredBillingStatements
                .Where(b =>(!string.IsNullOrEmpty(b.BillingNo) &&b.BillingNo.Contains(search,StringComparison.OrdinalIgnoreCase)) ||
                        (b.Tenant != null && ( ($"{b.Tenant.FirstName} {b.Tenant.LastName}").Contains(search,StringComparison.OrdinalIgnoreCase))) ||
                     (b.Unit != null && !string.IsNullOrEmpty(b.Unit.UnitNumber) && b.Unit.UnitNumber.Contains(search,StringComparison.OrdinalIgnoreCase))
                );
        }

        // APPLY STATUS FILTER

        if (!string.IsNullOrWhiteSpace(StatusFilter) &&
            !StatusFilter.Equals(
                "All",
                StringComparison.OrdinalIgnoreCase))
        {
            filteredBillingStatements =
                filteredBillingStatements.Where(b =>
                    string.Equals(
                        b.BillingStatus,
                        StatusFilter,
                        StringComparison.OrdinalIgnoreCase));
        }

        // APPLY BILLING MONTH FILTER

        if (!string.IsNullOrWhiteSpace(BillingMonthFilter) &&
            DateTime.TryParseExact(
                BillingMonthFilter + "-01",
                "yyyy-MM-dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var selectedMonth))
        {
            filteredBillingStatements =
                filteredBillingStatements.Where(b =>
                    b.BillingMonth.Year == selectedMonth.Year &&
                    b.BillingMonth.Month == selectedMonth.Month);
        }

        // FINAL BILLING STATEMENT LIST

        BillingStatement = filteredBillingStatements.ToList();
    }

    public class RecordPaymentRequest
    {
        public int BillingStatementID { get; set; }
        public int TenantID { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public DateTime PaymentDate { get; set; }
        public string? Remarks { get; set; }
    }

    public async Task<IActionResult> OnPostRecordPaymentAsync(
    [FromBody] RecordPaymentRequest request)
    {
        // 1. Validate payment request
        if (request == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Invalid payment information."
            })
            {
                StatusCode = 400
            };
        }

        // 2. Find the billing statement
        var billingStatement = await _context.BillingStatements
            .FirstOrDefaultAsync(b =>
                b.BillingStatementID == request.BillingStatementID);

        if (billingStatement == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Billing statement was not found."
            })
            {
                StatusCode = 404
            };
        }

        // 3. Validate payment amount
        if (request.AmountPaid <= 0)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Payment amount must be greater than zero."
            })
            {
                StatusCode = 400
            };
        }

        // 4. Prevent payment if the bill is already paid
        if (billingStatement.BillingStatus == "Paid")
        {
            return new JsonResult(new
            {
                success = false,
                message = "This billing statement is already paid."
            })
            {
                StatusCode = 400
            };
        }

        // 5. Get the total amount already applied to this bill
        decimal totalPaid =
            await _context.RentalPayments
                .Where(p =>
                    p.BillingStatementID ==
                    billingStatement.BillingStatementID)
                .SumAsync(p =>
                    (decimal?)p.AppliedAmount) ?? 0;

        // 6. Calculate the remaining balance
        decimal remainingBalance =
            billingStatement.TotalAmountDue - totalPaid;

        // Prevent negative balance
        if (remainingBalance < 0)
        {
            remainingBalance = 0;
        }

        // 7. Determine how much of this payment goes to the current bill
        decimal amountAppliedToBill =
            Math.Min(request.AmountPaid, remainingBalance);

        // 8. Determine excess payment / tenant credit
        decimal excessPayment =
            request.AmountPaid - amountAppliedToBill;

        // 9. Generate receipt number
        string receiptNo =
            $"RCPT-{DateTime.Now:yyyyMMddHHmmssfff}";

        // 10. Create payment record
        var payment = new RentalPayment
        {
            BillingStatementID =
                billingStatement.BillingStatementID,

            TenantID =
                billingStatement.TenantID,

            // Actual amount received from tenant
            AmountPaid =
                request.AmountPaid,

            // Amount actually applied to this billing statement
            AppliedAmount =
                amountAppliedToBill,

            PaymentDate =
                request.PaymentDate,

            PaymentMethod =
                request.PaymentMethod,

            PaymentStatus =
                "Completed",

            PaymentFor =
                billingStatement.BillingNo,

            BillingMonth =
                billingStatement.BillingMonth,

            Remarks =
                request.Remarks,

            ReceiptNo =
                receiptNo
        };

        // 11. Add payment
        _context.RentalPayments.Add(payment);

        // 12. Calculate new total paid toward this bill
        decimal newTotalPaid =
            totalPaid + amountAppliedToBill;

        // 13. Calculate remaining balance after payment
        decimal newRemainingBalance =
            billingStatement.TotalAmountDue - newTotalPaid;

        if (newRemainingBalance < 0)
        {
            newRemainingBalance = 0;
        }

        // 14. Update billing status
        if (newRemainingBalance <= 0)
        {
            billingStatement.BillingStatus = "Paid";
        }
        else
        {
            billingStatement.BillingStatus = "Partially Paid";
        }

        // 15. If there is an excess payment,
        //     create TenantCredit
        if (excessPayment > 0)
        {
            var tenantCredit = new TenantCredit
            {
                TenantID =
                    billingStatement.TenantID,

                Amount =
                    excessPayment,

                RemainingAmount =
                    excessPayment,

                DateCreated =
                    DateTime.Now,

                SourceReceiptNo =
                    receiptNo,

                Remarks =
                    $"Credit from overpayment of {billingStatement.BillingNo}"
            };

            _context.TenantCredits.Add(tenantCredit);
        }

        // 16. Save everything
        await _context.SaveChangesAsync();

        // 17. Return payment result
        return new JsonResult(new
        {
            success = true,

            message = excessPayment > 0
                ? $"Payment recorded successfully. ₱{excessPayment:N2} has been added as tenant credit."
                : "Payment recorded successfully.",

            receiptNo = receiptNo,

            amountPaid = request.AmountPaid,

            amountAppliedToBill = amountAppliedToBill,

            excessPayment = excessPayment,

            totalPaid = newTotalPaid,

            remainingBalance = newRemainingBalance,

            billingStatus = billingStatement.BillingStatus
        });
    }

    public async Task<IActionResult> OnPostMarkAsUnpaidAsync(
    int billingStatementId)
    {
        var billing =
            await _context.BillingStatements
                .FirstOrDefaultAsync(b =>
                    b.BillingStatementID ==
                    billingStatementId);

        if (billing == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Billing statement not found."
            })
            {
                StatusCode = 404
            };
        }

        // Only change Pending bills to Unpaid.
        // Do not overwrite Paid or Overdue.
        if (billing.BillingStatus == "Pending")
        {
            billing.BillingStatus = "Unpaid";

            await _context.SaveChangesAsync();
        }

        return new JsonResult(new
        {
            success = true,
            status = billing.BillingStatus
        });
    }

    public async Task<IActionResult> OnGetPaymentDetailsAsync(
     int billingStatementId)
    {
        var payment = await _context.RentalPayments
            .Include(p => p.Tenant)
            .Include(p => p.BillingStatement)
            .Where(p => p.BillingStatementID == billingStatementId)
            .OrderByDescending(p => p.PaymentDate)
            .FirstOrDefaultAsync();

        if (payment == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "No payment record was found."
            });
        }

        return new JsonResult(new
        {
            success = true,

            paymentID = payment.PaymentID,

            receiptNo = payment.ReceiptNo,

            tenantName = payment.Tenant != null
                ? payment.Tenant.FirstName + " " +
                  payment.Tenant.LastName
                : "N/A",

            billingNo = payment.BillingStatement != null
                ? payment.BillingStatement.BillingNo
                : "N/A",

            amountPaid = payment.AmountPaid,

            paymentDate = payment.PaymentDate
                .ToString("MMMM dd, yyyy"),

            paymentMethod = payment.PaymentMethod,

            paymentStatus = payment.PaymentStatus,

            paymentFor = payment.PaymentFor,

            billingMonth = payment.BillingMonth
                .ToString("MMMM yyyy"),

            remarks = payment.Remarks ?? "None"
        });
    }
    public async Task<IActionResult> OnGetPaymentSummaryAsync(
    int billingStatementId)
    {
        var billing =
            await _context.BillingStatements
                .FirstOrDefaultAsync(
                    b => b.BillingStatementID == billingStatementId);

        if (billing == null)
        {
            return new JsonResult(new
            {
                success = false,
                message = "Billing statement not found."
            })
            {
                StatusCode = 404
            };
        }

        decimal totalPaid =
            await _context.RentalPayments
                .Where(p =>
                    p.BillingStatementID ==
                    billingStatementId)
                .SumAsync(p =>
                    (decimal?)p.AppliedAmount) ?? 0;

        decimal remainingBalance =
            billing.TotalAmountDue -
            totalPaid;

        if (remainingBalance < 0)
            remainingBalance = 0;

        return new JsonResult(new
        {
            success = true,
            totalAmountDue =
                billing.TotalAmountDue,

            totalPaid =
                totalPaid,

            remainingBalance =
                remainingBalance
        });
    }
}
