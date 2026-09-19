using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.Data;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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
        UtilityReading = utilityreading;
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

        _context.Attach(UtilityReading).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!UtilityReadingExists(UtilityReading.UtilityReadingID))
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

    private bool UtilityReadingExists(int utilityreadingid)
    {
        return _context.UtilityReadings.Any(e => e.UtilityReadingID == utilityreadingid);
    }
}
