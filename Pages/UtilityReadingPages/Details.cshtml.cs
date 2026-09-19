using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public UtilityReading UtilityReading { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? utilityreadingid)
    {
        if (utilityreadingid is null)
        {
            return NotFound();
        }

        var utilityreading = await _context.UtilityReadings.FirstOrDefaultAsync(m => m.UtilityReadingID == utilityreadingid);
        if (utilityreading is null)
        {
            return NotFound();
        }
        else
        {
            UtilityReading = utilityreading;
        }

        return Page();
    }
}
