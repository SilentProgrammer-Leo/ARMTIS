using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.Pages.UnitPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Unit Unit { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var unit = await _context.Units.FirstOrDefaultAsync(m => m.UnitID == id);
        if (unit is null)
        {
            return NotFound();
        }
        else
        {
            Unit = unit;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var unit = await _context.Units.FindAsync(id);
        if (unit != null)
        {
            Unit = unit;
            _context.Units.Remove(Unit);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
