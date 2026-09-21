using System.IO;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPortal.Profile
{
    [Authorize(Roles = "Tenant")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _environment;

        public IndexModel(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment environment)
        {
            _context = context;
            _userManager = userManager;
            _environment = environment;
        }


        // =========================================================
        // EDITABLE PROFILE INFORMATION
        // =========================================================

        [BindProperty]
        public string FirstName { get; set; } = string.Empty;

        [BindProperty]
        public string LastName { get; set; } = string.Empty;

        [BindProperty]
        public string Address { get; set; } = string.Empty;

        [BindProperty]
        public string ContactNo { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;


        // =========================================================
        // PROFILE IMAGE
        // =========================================================

        [BindProperty]
        public IFormFile? ProfileImage { get; set; }

        public string? ProfileImagePath { get; set; }


        // =========================================================
        // DISPLAY DATA
        // =========================================================

        public Tenant? Tenant { get; set; }

        public ApplicationUser? UserAccount { get; set; }


        // =========================================================
        // GET
        // =========================================================

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (user.TenantID == null)
            {
                return Forbid();
            }

            UserAccount = user;

            Tenant = await _context.Tenants
                .Include(t => t.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.TenantID == user.TenantID.Value);

            if (Tenant == null)
            {
                return NotFound();
            }

            // Populate editable fields
            FirstName = Tenant.FirstName;
            LastName = Tenant.LastName;
            Address = Tenant.Address ?? string.Empty;
            ContactNo = Tenant.ContactNo ?? string.Empty;

            // Use the Tenant email first, then Identity email as fallback
            Email = !string.IsNullOrWhiteSpace(Tenant.Email)
                ? Tenant.Email
                : user.Email ?? string.Empty;

            ProfileImagePath = Tenant.ProfileImagePath;

            return Page();
        }


        // =========================================================
        // POST - SAVE PROFILE
        // =========================================================

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return RedirectToPage("/Account/Login");
            }

            if (user.TenantID == null)
            {
                return Forbid();
            }


            // Load the tracked tenant for updating
            Tenant = await _context.Tenants
                .Include(t => t.Unit)
                .FirstOrDefaultAsync(
                    t => t.TenantID == user.TenantID.Value);

            if (Tenant == null)
            {
                return NotFound();
            }

            UserAccount = user;


            // =====================================================
            // BASIC VALIDATION
            // =====================================================

            FirstName = FirstName?.Trim() ?? string.Empty;
            LastName = LastName?.Trim() ?? string.Empty;
            Address = Address?.Trim() ?? string.Empty;
            ContactNo = ContactNo?.Trim() ?? string.Empty;
            Email = Email?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(FirstName))
            {
                ModelState.AddModelError(
                    nameof(FirstName),
                    "First name is required.");
            }

            if (string.IsNullOrWhiteSpace(LastName))
            {
                ModelState.AddModelError(
                    nameof(LastName),
                    "Last name is required.");
            }

            if (!string.IsNullOrWhiteSpace(Email) &&
                !new System.ComponentModel.DataAnnotations.EmailAddressAttribute()
                    .IsValid(Email))
            {
                ModelState.AddModelError(
                    nameof(Email),
                    "Please enter a valid email address.");
            }


            // =====================================================
            // PROFILE IMAGE VALIDATION
            // =====================================================

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                const long maxFileSize = 5 * 1024 * 1024; // 5 MB

                var allowedExtensions = new[]
                {
                    ".jpg",
                    ".jpeg",
                    ".png"
                };

                var extension = Path
                    .GetExtension(ProfileImage.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError(
                        nameof(ProfileImage),
                        "Only JPG, JPEG, and PNG images are allowed.");
                }

                if (ProfileImage.Length > maxFileSize)
                {
                    ModelState.AddModelError(
                        nameof(ProfileImage),
                        "The profile image must not exceed 5 MB.");
                }
            }


            // =====================================================
            // RETURN PAGE IF VALIDATION FAILS
            // =====================================================

            if (!ModelState.IsValid)
            {
                ProfileImagePath = Tenant.ProfileImagePath;
                return Page();
            }


            // =====================================================
            // UPDATE TENANT INFORMATION
            // =====================================================

            Tenant.FirstName = FirstName;
            Tenant.LastName = LastName;
            Tenant.Address = Address;
            Tenant.ContactNo = ContactNo;
            Tenant.Email = Email;


            // =====================================================
            // UPDATE IDENTITY EMAIL
            // =====================================================

            if (!string.Equals(
                    user.Email,
                    Email,
                    StringComparison.OrdinalIgnoreCase))
            {
                var emailResult = await _userManager.SetEmailAsync(
                    user,
                    string.IsNullOrWhiteSpace(Email)
                        ? null
                        : Email);

                if (!emailResult.Succeeded)
                {
                    foreach (var error in emailResult.Errors)
                    {
                        ModelState.AddModelError(
                            nameof(Email),
                            error.Description);
                    }

                    ProfileImagePath = Tenant.ProfileImagePath;
                    return Page();
                }
            }


            // =====================================================
            // PROFILE IMAGE UPLOAD
            // =====================================================

            if (ProfileImage != null && ProfileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(
                    _environment.WebRootPath,
                    "uploads",
                    "tenants");

                Directory.CreateDirectory(uploadsFolder);


                // Generate a unique filename
                var extension = Path
                    .GetExtension(ProfileImage.FileName)
                    .ToLowerInvariant();

                var fileName =
                    $"{Tenant.TenantID}_{Guid.NewGuid():N}{extension}";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);


                // Save new image
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await ProfileImage.CopyToAsync(stream);
                }


                // Save relative path to database
                var oldImagePath = Tenant.ProfileImagePath;

                Tenant.ProfileImagePath =
                    $"/uploads/tenants/{fileName}";

                ProfileImagePath = Tenant.ProfileImagePath;


                // Delete previous image if it belongs to our upload folder
                if (!string.IsNullOrWhiteSpace(oldImagePath) &&
                    oldImagePath.StartsWith(
                        "/uploads/tenants/",
                        StringComparison.OrdinalIgnoreCase))
                {
                    var oldFileName =
                        Path.GetFileName(oldImagePath);

                    var oldFilePath = Path.Combine(
                        uploadsFolder,
                        oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }
            else
            {
                ProfileImagePath = Tenant.ProfileImagePath;
            }


            // =====================================================
            // SAVE DATABASE CHANGES
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // SUCCESS MESSAGE
            // =====================================================

            TempData["SuccessMessage"] =
                "Your profile information has been updated successfully.";

            return RedirectToPage();
        }
    }
}