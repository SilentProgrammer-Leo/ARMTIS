using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPages.Account
{
    [Authorize(Roles = "Admin")]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CreateModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; } = new();

        public SelectList TenantList { get; set; } = default!;

        public class InputModel
        {
            public int TenantID { get; set; }

            public string Username { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            public string ConfirmPassword { get; set; } = string.Empty;
        }

        public async Task OnGetAsync()
        {
            await LoadTenantListAsync();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            await LoadTenantListAsync();

            if (Input.TenantID <= 0)
            {
                ModelState.AddModelError(
                    "Input.TenantID",
                    "Please select a tenant.");
            }

            if (string.IsNullOrWhiteSpace(Input.Username))
            {
                ModelState.AddModelError(
                    "Input.Username",
                    "Username is required.");
            }

            if (string.IsNullOrWhiteSpace(Input.Password))
            {
                ModelState.AddModelError(
                    "Input.Password",
                    "Password is required.");
            }

            if (Input.Password != Input.ConfirmPassword)
            {
                ModelState.AddModelError(
                    "Input.ConfirmPassword",
                    "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            // Check if tenant exists
            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantID == Input.TenantID);

            if (tenant == null)
            {
                ModelState.AddModelError(
                    "Input.TenantID",
                    "Selected tenant does not exist.");

                return Page();
            }

            // Check if tenant already has an account
            var existingTenantAccount = await _userManager.Users
                .FirstOrDefaultAsync(u => u.TenantID == Input.TenantID);

            if (existingTenantAccount != null)
            {
                ModelState.AddModelError(
                    "Input.TenantID",
                    "This tenant already has a login account.");

                return Page();
            }

            // Check username
            var existingUsername =
                await _userManager.FindByNameAsync(Input.Username);

            if (existingUsername != null)
            {
                ModelState.AddModelError(
                    "Input.Username",
                    "This username is already taken.");

                return Page();
            }

            // Create Identity user
            var user = new ApplicationUser
            {
                UserName = Input.Username,
                TenantID = Input.TenantID,
                Email = null,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(
                user,
                Input.Password);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }

            // Assign Tenant role
            var roleResult = await _userManager.AddToRoleAsync(
                user,
                "Tenant");

            if (!roleResult.Succeeded)
            {
                // Remove account if role assignment fails
                await _userManager.DeleteAsync(user);

                foreach (var error in roleResult.Errors)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        error.Description);
                }

                return Page();
            }

            TempData["SuccessMessage"] =
                $"Account successfully created for {tenant.FirstName} {tenant.LastName}.";

            return RedirectToPage("/TenantPages/Accounts");
        }

        private async Task LoadTenantListAsync()
        {
            var tenants = await _context.Tenants
                .OrderBy(t => t.FirstName)
                .ThenBy(t => t.LastName)
                .Select(t => new
                {
                    t.TenantID,
                    FullName = t.FirstName + " " + t.LastName
                })
                .ToListAsync();

            TenantList = new SelectList(
                tenants,
                "TenantID",
                "FullName");
        }
    }
}
