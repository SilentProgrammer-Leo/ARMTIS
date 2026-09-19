using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.Services;

namespace ARMTIS_Capstone_Project.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuditLogService _auditLogService;

        public LogoutModel(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            AuditLogService auditLogService)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _auditLogService = auditLogService;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var currentUser = await _userManager.GetUserAsync(User);
            string? role = null;

            if (currentUser != null)
            {
                var roles = await _userManager.GetRolesAsync(currentUser);
                role = roles.FirstOrDefault();
            }

            await _auditLogService.LogAsync(
                "Logout",
                "User logged out.",
                currentUser?.Id,
                currentUser?.UserName,
                role);

            await _signInManager.SignOutAsync();

            return RedirectToPage("/Account/Login");
        }

        public void OnGet()
        {
        }
    }
}
