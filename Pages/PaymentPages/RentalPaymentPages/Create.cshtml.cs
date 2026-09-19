using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;
using Microsoft.AspNetCore.Mvc.Rendering;


namespace ARMTIS_Capstone_Project.Pages.PaymentPages.RentalPaymentPages;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult OnGet()
    {
        ViewData["TenantID"] = new SelectList(
     _context.Tenants
         .Select(t => new
         {
             t.TenantID,
             Name = t.FirstName + " " + t.LastName
         }),
     "TenantID",
     "Name");

        return Page();
    }

    [BindProperty]
    public RentalPayment RentalPayment { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            ViewData["TenantID"] = new SelectList(
       _context.Tenants
           .Select(t => new
           {
               t.TenantID,
               FullName = t.FirstName + " " + t.LastName
           }),
       "TenantID",
       "FullName");
            return Page();
        }

        _context.RentalPayments.Add(RentalPayment);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
