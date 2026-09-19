using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Mvc;
using ARMTIS_Capstone_Project.Data;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.Payment_History
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

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
