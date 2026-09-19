using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.BillingStatementPages;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

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
}
