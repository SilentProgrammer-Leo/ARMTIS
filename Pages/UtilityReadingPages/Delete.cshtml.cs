using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
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

    public async Task<IActionResult> OnPostAsync(int? utilityreadingid)
    {
        if (utilityreadingid is null)
        {
            return NotFound();
        }

        var utilityreading = await _context.UtilityReadings.FindAsync(utilityreadingid);
        if (utilityreading != null)
        {
            UtilityReading = utilityreading;
            _context.UtilityReadings.Remove(UtilityReading);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
