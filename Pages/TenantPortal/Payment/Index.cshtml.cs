using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Payment
{
    [Authorize(Roles = "Tenant")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<RentalPayment> Payments { get; set; }
            = new List<RentalPayment>();

        public decimal TotalAmountPaid { get; set; }

        public int TotalPayments { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            // Get the currently logged-in tenant
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            // Tenant account must be linked to a Tenant record
            if (user.TenantID == null)
            {
                return Forbid();
            }

            int tenantID = user.TenantID.Value;

            // Get ONLY payments belonging to the logged-in tenant
            Payments = await _context.RentalPayments
                .AsNoTracking()
                .Where(p => p.TenantID == tenantID)
                .OrderByDescending(p => p.PaymentDate)
                .ThenByDescending(p => p.PaymentID)
                .ToListAsync();

            // Summary information
            TotalPayments = Payments.Count;

            TotalAmountPaid = Payments.Sum(p => p.AmountPaid);

            return Page();
        }
    }
}