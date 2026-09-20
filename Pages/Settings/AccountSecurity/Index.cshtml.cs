using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Settings.AccountSecurity
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public List<UserAccountViewModel> Accounts { get; set; } = new();

        [BindProperty]
        public ResetPasswordInput Input { get; set; } = new();

        public async Task OnGetAsync()
        {
            await LoadAccountsAsync();
        }

        public async Task<IActionResult> OnPostResetPasswordAsync()
        {
            await LoadAccountsAsync();

            if (string.IsNullOrWhiteSpace(Input.UserId))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected account could not be identified.");

                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.NewPassword))
            {
                ModelState.AddModelError(
                    "Input.NewPassword",
                    "New password is required.");
            }

            if (string.IsNullOrWhiteSpace(Input.ConfirmPassword))
            {
                ModelState.AddModelError(
                    "Input.ConfirmPassword",
                    "Please confirm the new password.");
            }

            if (Input.NewPassword != Input.ConfirmPassword)
            {
                ModelState.AddModelError(
                    "Input.ConfirmPassword",
                    "Passwords do not match.");
            }

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.Users
                .Include(u => u.Tenant)
                .FirstOrDefaultAsync(u =>
                    u.Id == Input.UserId);

            if (user == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "The selected account no longer exists.");

                return Page();
            }

            // Generate an Identity password-reset token.
            var token = await _userManager
                .GeneratePasswordResetTokenAsync(user);

            var result = await _userManager.ResetPasswordAsync(
                user,
                token,
                Input.NewPassword);

            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(
                        "Input.NewPassword",
                        error.Description);
                }

                return Page();
            }

            var displayName = user.Tenant != null
                ? $"{user.Tenant.FirstName} {user.Tenant.LastName}"
                : user.UserName ?? "Administrator";

            TempData["SuccessMessage"] =
                $"Password successfully updated for {displayName} ({user.UserName}).";

            return RedirectToPage();
        }

        private async Task LoadAccountsAsync()
        {
            var users = await _userManager.Users
                .Include(u => u.Tenant)
                .AsNoTracking()
                .OrderBy(u => u.UserName)
                .ToListAsync();

            Accounts = new List<UserAccountViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                string displayName;

                if (user.Tenant != null)
                {
                    displayName =
                        $"{user.Tenant.FirstName} {user.Tenant.LastName}";
                }
                else
                {
                    displayName = "Administrator";
                }

                Accounts.Add(new UserAccountViewModel
                {
                    UserId = user.Id,
                    UserName = user.UserName ?? "N/A",
                    DisplayName = displayName,
                    Roles = roles.Count > 0
                        ? string.Join(", ", roles)
                        : "No Role",
                    HasPassword = !string.IsNullOrWhiteSpace(
                        user.PasswordHash)
                });
            }
        }

        public class UserAccountViewModel
        {
            public string UserId { get; set; } = string.Empty;

            public string UserName { get; set; } = string.Empty;

            public string DisplayName { get; set; } = string.Empty;

            public string Roles { get; set; } = string.Empty;

            public bool HasPassword { get; set; }
        }

        public class ResetPasswordInput
        {
            public string UserId { get; set; } = string.Empty;

            public string NewPassword { get; set; } = string.Empty;

            public string ConfirmPassword { get; set; } = string.Empty;
        }
    }
}