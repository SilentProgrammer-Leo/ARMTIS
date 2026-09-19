using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.Pages.TenantPages;

public class DeleteModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DeleteModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Tenant Tenant { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants.FirstOrDefaultAsync(m => m.TenantID == id);
        if (tenant is null)
        {
            return NotFound();
        }
        else
        {
            Tenant = tenant;
        }

        return Page();
    }

    public async Task<IActionResult> OnPostAsync(int? id)
    {
        if (id is null)
        {
            return NotFound();
        }

        var tenant = await _context.Tenants.FindAsync(id);
        if (tenant != null)
        {
            Tenant = tenant;
            _context.Tenants.Remove(Tenant);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage("./Index");
    }
}
