using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ARMTIS_Capstone_Project.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;

        public LoginModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditLogService auditLogService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public class LoginInputModel
        {
            public string Username { get; set; } = string.Empty;

            public string Password { get; set; } = string.Empty;

            public bool RememberMe { get; set; }
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
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

            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByNameAsync(Input.Username);

            if (user == null)
            {
                await _auditLogService.LogAsync(
                    "Login Failed",
                    $"Failed login attempt for username {Input.Username}.",
                    userName: Input.Username,
                    userRole: "Unknown");

                ErrorMessage = "Invalid username or password.";
                return Page();
            }

            var result = await _signInManager.PasswordSignInAsync(
                user.UserName!,
                Input.Password,
                Input.RememberMe,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
                var isTenant = await _userManager.IsInRoleAsync(user, "Tenant");

                if (isAdmin)
                {
                    await _auditLogService.LogAsync(
                        "Login",
                        "Administrator logged in successfully.",
                        user.Id,
                        user.UserName,
                        "Admin");

                    return RedirectToPage("/Index");
                }

                if (isTenant)
                {
                    await _auditLogService.LogAsync(
                        "Login",
                        "Tenant logged in successfully.",
                        user.Id,
                        user.UserName,
                        "Tenant");

                    return RedirectToPage("/TenantPortal/Dashboard/Index");
                }

                await _auditLogService.LogAsync(
                    "Login Blocked",
                    "Login succeeded but the account has no assigned application role.",
                    user.Id,
                    user.UserName,
                    "Unassigned");

                await _signInManager.SignOutAsync();

                ErrorMessage = "Your account does not have an assigned role.";

                return Page();
            }

            if (result.IsLockedOut)
            {
                ErrorMessage = "Your account is temporarily locked.";
            }
            else
            {
                ErrorMessage = "Invalid username or password.";
            }

            await _auditLogService.LogAsync(
                "Login Failed",
                $"Failed login attempt for username {Input.Username}.",
                user.Id,
                user.UserName,
                "Unknown");

            return Page();
        }
    }
}