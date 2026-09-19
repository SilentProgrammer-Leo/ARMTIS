using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages
{
    public class UtilityRateIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UtilityRateIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<UtilityRate> UtilityRates { get; set; }
           = new List<UtilityRate>();

        public async Task OnGetAsync()
        {
            UtilityRates = await _context.UtilityRates
                .OrderBy(r => r.UtilityType)
                .ThenByDescending(r => r.EffectiveFrom)
                .ToListAsync();
        }

       
    }
}
