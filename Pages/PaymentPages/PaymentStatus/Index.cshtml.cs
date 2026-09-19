using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.Payment_Status
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // This goes here
        public IList<RentalPayment> Payments { get; set; }
            = new List<RentalPayment>();

        public async Task OnGetAsync()
        {
            Payments = await _context.RentalPayments
                .Include(p => p.Tenant)
                .ToListAsync();
        }
    }
}
