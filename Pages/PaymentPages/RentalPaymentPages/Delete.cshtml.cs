using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.RentalPaymentPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public RentalPayment RentalPayment { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? paymentid)
    {
        if (paymentid is null)
        {
            return NotFound();
        }

        var rentalpayment = await _context.RentalPayments.FirstOrDefaultAsync(m => m.PaymentID == paymentid);
        if (rentalpayment is null)
        {
            return NotFound();
        }
        else
        {
            RentalPayment = rentalpayment;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? paymentid)
    {
        if (paymentid is null)
        {
            return NotFound();
        }

        var rentalpayment = await _context.RentalPayments.FindAsync(paymentid);
        if (rentalpayment != null)
        {
            RentalPayment = rentalpayment;
            _context.RentalPayments.Remove(RentalPayment);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
