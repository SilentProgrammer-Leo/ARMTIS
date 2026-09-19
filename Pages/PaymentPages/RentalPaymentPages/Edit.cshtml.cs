using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.RentalPaymentPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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
        RentalPayment = rentalpayment;
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

        _context.Attach(RentalPayment).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!RentalPaymentExists(RentalPayment.PaymentID))
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

    private bool RentalPaymentExists(int paymentid)
    {
        return _context.RentalPayments.Any(e => e.PaymentID == paymentid);
    }
}
