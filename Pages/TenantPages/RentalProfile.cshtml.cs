using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPages
{
    public class RentalProfileModel : PageModel
    {

        [BindProperty]
        public IFormFile? RentalAgreementImage { get; set; }
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RentalProfileModel(
     ApplicationDbContext context,
     IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public Tenant Tenant { get; set; } = null!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.TenantID == id.Value);

            if (tenant == null)
            {
                return NotFound();
            }

            Tenant = tenant;

            return Page();
        }
        public async Task<IActionResult> OnPostUploadAgreementAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (RentalAgreementImage == null ||
                RentalAgreementImage.Length == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select a rental agreement image.";

                return RedirectToPage(
                    "./RentalProfile",
                    new { id = id.Value });
            }

            const long maxFileSize = 10 * 1024 * 1024; // 10 MB

            if (RentalAgreementImage.Length > maxFileSize)
            {
                TempData["ErrorMessage"] =
                    "The rental agreement image must not exceed 10 MB.";

                return RedirectToPage(
                    "./RentalProfile",
                    new { id = id.Value });
            }

            var allowedExtensions = new[]
            {
        ".jpg",
        ".jpeg",
        ".png"
    };

            var extension = Path
                .GetExtension(RentalAgreementImage.FileName)
                .ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
            {
                TempData["ErrorMessage"] =
                    "Only JPG, JPEG, and PNG images are allowed.";

                return RedirectToPage(
                    "./RentalProfile",
                    new { id = id.Value });
            }

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(t => t.TenantID == id.Value);

            if (tenant == null)
            {
                return NotFound();
            }

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "tenants",
                "agreements");

            Directory.CreateDirectory(uploadsFolder);

            var fileName =
                $"agreement_{tenant.TenantID}_{Guid.NewGuid():N}{extension}";

            var filePath = Path.Combine(
                uploadsFolder,
                fileName);

            await using (var stream = new FileStream(
                filePath,
                FileMode.Create))
            {
                await RentalAgreementImage.CopyToAsync(stream);
            }

            // Delete previous agreement image
            if (!string.IsNullOrWhiteSpace(
                tenant.RentalAgreementImagePath))
            {
                if (tenant.RentalAgreementImagePath.StartsWith(
                    "/uploads/tenants/agreements/",
                    StringComparison.OrdinalIgnoreCase))
                {
                    var oldFileName =
                        Path.GetFileName(
                            tenant.RentalAgreementImagePath);

                    var oldFilePath = Path.Combine(
                        uploadsFolder,
                        oldFileName);

                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }
            }

            tenant.RentalAgreementImagePath =
                $"/uploads/tenants/agreements/{fileName}";

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] =
                "Rental agreement uploaded successfully.";

            return RedirectToPage(
                "./RentalProfile",
                new { id = tenant.TenantID });
        }
    }
}