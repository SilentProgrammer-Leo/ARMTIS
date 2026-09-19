using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.PaymentPages.RentalPaymentPages
{
    public class ReceiptModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ReceiptModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public RentalPayment RentalPayment { get; set; }
              = default!;

        public async Task<IActionResult> OnGetAsync(
            int? paymentid)
        {
            if (paymentid == null)
            {
                return NotFound();
            }

            var payment =
                await _context.RentalPayments
                    .Include(p => p.Tenant)
                    .Include(p => p.BillingStatement)
                    .FirstOrDefaultAsync(p =>
                        p.PaymentID == paymentid);

            if (payment == null)
            {
                return NotFound();
            }

            RentalPayment = payment;

            return Page();
        }
    }
}
