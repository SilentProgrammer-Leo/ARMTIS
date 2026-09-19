using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.BillingStatementPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public BillingStatement BillingStatement { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? billingstatementid)
    {
        if (billingstatementid is null)
        {
            return NotFound();
        }

        var billingstatement = await _context.BillingStatements.FirstOrDefaultAsync(m => m.BillingStatementID == billingstatementid);
        if (billingstatement is null)
        {
            return NotFound();
        }
        else
        {
            BillingStatement = billingstatement;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? billingstatementid)
    {
        if (billingstatementid == null)
        {
            return NotFound();
        }

        var billingStatement =
            await _context.BillingStatements
                .FindAsync(billingstatementid);

        if (billingStatement == null)
        {
            return NotFound();
        }

        bool hasPayment =
            await _context.RentalPayments
                .AnyAsync(p =>
                    p.BillingStatementID ==
                    billingstatementid.Value);

        if (hasPayment)
        {
            TempData["ErrorMessage"] =
                "This billing statement cannot be deleted because it already has a recorded payment.";

            return RedirectToPage("./Index");
        }

        _context.BillingStatements.Remove(
            billingStatement);

        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] =
            "Billing statement deleted successfully.";

        return RedirectToPage("./Index");
    }
}
