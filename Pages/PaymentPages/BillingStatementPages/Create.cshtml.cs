using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using ARMTIS_Capstone_Project.Services;


namespace ARMTIS_Capstone_Project.Pages.BillingStatementPages;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly NotificationService _notificationService;

    public CreateModel(
        ApplicationDbContext context,
        NotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        ViewData["UnitID"] = new SelectList(
        await _context.Units
            .Where(u => u.UnitStatus == "Occupied")
            .ToListAsync(),
        "UnitID",
        "UnitNumber");

        var lastBilling = await _context.BillingStatements
            .OrderByDescending(b => b.BillingStatementID)
            .FirstOrDefaultAsync();

        int nextNumber = 1;

        if (lastBilling != null)
            nextNumber = lastBilling.BillingStatementID + 1;

        BillingStatement = new BillingStatement
        {
            BillingNo = $"BILL-{DateTime.Now:yyyy}-{nextNumber:D5}",
            BillingStatus = "Pending",
            DateGenerated = DateTime.Today,
            DueDate = DateTime.Today.AddDays(5)
        };

        return Page();
    }

    [BindProperty]
    public BillingStatement BillingStatement { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD.
    public async Task < IActionResult > OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        ViewData["UnitID"] = new SelectList(
            await _context.Units
                .Where(u => u.UnitStatus == "Occupied")
                .ToListAsync(),
            "UnitID",
            "UnitNumber",
            BillingStatement.UnitID);

        return Page();
}
        BillingStatement.TotalAmountDue =
        BillingStatement.MonthlyRent +
        BillingStatement.UtilityCharges +
        BillingStatement.PenaltyAmount;
        _context.BillingStatements.Add(BillingStatement);

    await _context.SaveChangesAsync();

    await _notificationService.NotifyTenantAsync(
        BillingStatement.TenantID,
        "New Billing Statement",
        $"Your billing statement for {BillingStatement.BillingMonth:MMMM yyyy} has been generated. Billing No.: {BillingStatement.BillingNo}. Please check your Bills page for the amount due and due date.",
        "Billing");

    return RedirectToPage("./Index");
}

    public async Task<JsonResult> OnGetUnitInfoAsync(int unitId)
    {
        var tenant = await _context.Tenants
            .Include(t => t.Unit)
            .FirstOrDefaultAsync(t => t.UnitID == unitId);

        if (tenant == null)
        {
            return new JsonResult(new
            {
                tenantID = 0,
                tenantName = "",
                monthlyRent = 0,
                billingMonth = "",
                dueDate = ""
            });
        }

        var billingMonth = tenant.LeaseStartDate.AddMonths(1);

        return new JsonResult(new
        {
            tenantID = tenant.TenantID,
            tenantName = tenant.FirstName + " " + tenant.LastName,
            monthlyRent = tenant.Unit!.MonthlyRent,
            billingMonth = billingMonth.ToString("yyyy-MM-dd"),
            dueDate = billingMonth.AddDays(5).ToString("yyyy-MM-dd")
        });
    }
}
