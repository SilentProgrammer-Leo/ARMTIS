using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.Pages.TenantPages;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
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

        var tenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.TenantID == id.Value);

        if (tenant is null)
        {
            return NotFound();
        }

        Tenant = tenant;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        // Load the existing tenant instead of marking the submitted
        // object as entirely modified.
        var existingTenant = await _context.Tenants
            .FirstOrDefaultAsync(t => t.TenantID == Tenant.TenantID);

        if (existingTenant is null)
        {
            return NotFound();
        }

        // Validate the tenant status.
        var requestedStatus = Tenant.Status?.Trim();

        if (string.IsNullOrWhiteSpace(requestedStatus) ||
            !(requestedStatus.Equals(
                  "Active",
                  StringComparison.OrdinalIgnoreCase)
              || requestedStatus.Equals(
                  "Inactive",
                  StringComparison.OrdinalIgnoreCase)))
        {
            ModelState.AddModelError(
                "Tenant.Status",
                "Please select a valid tenant status.");

            return Page();
        }

        // Ensure the selected unit exists.
        var selectedUnitExists = await _context.Units
            .AnyAsync(u => u.UnitID == Tenant.UnitID);

        if (!selectedUnitExists)
        {
            ModelState.AddModelError(
                "Tenant.UnitID",
                "The selected unit does not exist.");

            return Page();
        }

        // Save the old values before updating the tenant.
        var previousUnitId = existingTenant.UnitID;
        var previousStatus = existingTenant.Status;

        var newStatus = requestedStatus.Equals(
            "Inactive",
            StringComparison.OrdinalIgnoreCase)
                ? "Inactive"
                : "Active";

        var isInactive = newStatus == "Inactive";

        var statusChanged = !string.Equals(
            previousStatus,
            newStatus,
            StringComparison.OrdinalIgnoreCase);

        var unitChanged = previousUnitId != Tenant.UnitID;

        // -----------------------------------------
        // UPDATE TENANT INFORMATION
        // -----------------------------------------

        existingTenant.FirstName = Tenant.FirstName;
        existingTenant.LastName = Tenant.LastName;
        existingTenant.Address = Tenant.Address;
        existingTenant.ContactNo = Tenant.ContactNo;
        existingTenant.Email = Tenant.Email;
        existingTenant.Status = newStatus;
        existingTenant.LeaseStartDate = Tenant.LeaseStartDate;
        existingTenant.LeaseEndDate = Tenant.LeaseEndDate;
        existingTenant.UnitID = Tenant.UnitID;

        // -----------------------------------------
        // DISABLE OR RE-ENABLE TENANT LOGIN
        // -----------------------------------------

        // ApplicationUser must be linked to this tenant
        // through its TenantID property.
        var linkedUser = await _context.Users
            .FirstOrDefaultAsync(
                u => u.TenantID == existingTenant.TenantID);

        if (linkedUser is not null)
        {
            if (isInactive)
            {
                // Enable Identity lockout and set the lockout
                // end far into the future to block future logins.
                linkedUser.LockoutEnabled = true;

                if (linkedUser.LockoutEnd != DateTimeOffset.MaxValue)
                {
                    linkedUser.LockoutEnd = DateTimeOffset.MaxValue;

                    // Invalidate existing authentication cookies
                    // when Identity next validates the security stamp.
                    linkedUser.SecurityStamp = Guid.NewGuid().ToString();
                }
            }
            else if (statusChanged)
            {
                // The tenant has been reactivated.
                // Allow the tenant to sign in again.
                linkedUser.LockoutEnabled = true;
                linkedUser.LockoutEnd = null;
                linkedUser.SecurityStamp = Guid.NewGuid().ToString();
            }
        }

        // -----------------------------------------
        // UPDATE UNIT OCCUPANCY
        // -----------------------------------------

        // Recalculate occupancy if the tenant's status or
        // assigned unit has changed.
        if (statusChanged || unitChanged)
        {
            var affectedUnitIds = new[]
            {
                previousUnitId,
                existingTenant.UnitID
            }.Distinct();

            foreach (var unitId in affectedUnitIds)
            {
                var unit = await _context.Units
                    .FirstOrDefaultAsync(u => u.UnitID == unitId);

                if (unit is null)
                {
                    continue;
                }

                // Check whether another active tenant is
                // assigned to this unit.
                var anotherActiveTenantExists =
                    await _context.Tenants.AnyAsync(t =>
                        t.TenantID != existingTenant.TenantID &&
                        t.UnitID == unitId &&
                        t.Status == "Active");

                // Count the current tenant only if they are
                // active and still assigned to this unit.
                var currentTenantIsActiveInUnit =
                    !isInactive &&
                    existingTenant.UnitID == unitId;

                unit.UnitStatus =
                    anotherActiveTenantExists ||
                    currentTenantIsActiveInUnit
                        ? "Occupied"
                        : "Vacant";
            }
        }

        // -----------------------------------------
        // SAVE ALL CHANGES
        // -----------------------------------------

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await TenantExists(Tenant.TenantID))
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToPage("./Index");
    }

    private async Task<bool> TenantExists(int id)
    {
        return await _context.Tenants
            .AnyAsync(t => t.TenantID == id);
    }
}