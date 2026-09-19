using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.Pages.TenantPages;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public SelectList UnitList { get; set; }

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        var units = await _context.Units
    .Where(u => u.UnitStatus == "Vacant")
    .ToListAsync();

        UnitList = new SelectList(
            units,
            "UnitID",
            "UnitNumber");

        return Page();
    }

    [BindProperty]
    public Tenant Tenant { get; set; } = default!;

    // To protect from overposting attacks, see https://aka.ms/RazorPagesCRUD.
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            UnitList = new SelectList(
                await _context.Units.ToListAsync(),
                "UnitID",
                "UnitNumber");

            return Page();
        }

        _context.Tenants.Add(Tenant);

        var unit = await _context.Units.FindAsync(Tenant.UnitID);

        if (unit != null)
        {
            unit.UnitStatus = "Occupied";
        }

        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
}
