using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.RentalPaymentPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<RentalPayment> RentalPayment { get; set; } = default!;

    public async Task OnGetAsync()
    {
        RentalPayment = await _context.RentalPayments.ToListAsync();
    }
    public int TenantID { get; set; }

    public Tenant? Tenant { get; set; }
}
