using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.BillingStatementPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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
        BillingStatement = billingstatement;
        return Page();
    }

    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        _context.Attach(BillingStatement).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BillingStatementExists(BillingStatement.BillingStatementID))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private bool BillingStatementExists(int billingstatementid)
    {
        return _context.BillingStatements.Any(e => e.BillingStatementID == billingstatementid);
    }
}
