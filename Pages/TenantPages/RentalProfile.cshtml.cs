using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.TenantPages
{
    public class RentalProfileModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public RentalProfileModel(
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }


        // =========================================================
        // MULTIPLE RENTAL AGREEMENT IMAGES
        // =========================================================

        [BindProperty]
        public List<IFormFile> RentalAgreementImages { get; set; }
            = new List<IFormFile>();


        // =========================================================
        // TENANT
        // =========================================================

        public Tenant Tenant { get; set; } = null!;


        // =========================================================
        // GET
        // =========================================================

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var tenant = await _context.Tenants
                .Include(t => t.Unit)
                .Include(t => t.RentalAgreementImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(
                    t => t.TenantID == id.Value);

            if (tenant == null)
            {
                return NotFound();
            }

            Tenant = tenant;

            return Page();
        }


        // =========================================================
        // POST - UPLOAD RENTAL AGREEMENT PAGES
        // =========================================================

        public async Task<IActionResult> OnPostUploadAgreementAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            if (RentalAgreementImages == null ||
                RentalAgreementImages.Count == 0)
            {
                TempData["ErrorMessage"] =
                    "Please select at least one rental agreement image.";

                return RedirectToPage(
                    new { id = id.Value });
            }


            // =====================================================
            // LOAD TENANT
            // =====================================================

            var tenant = await _context.Tenants
                .FirstOrDefaultAsync(
                    t => t.TenantID == id.Value);

            if (tenant == null)
            {
                return NotFound();
            }


            // =====================================================
            // UPLOAD FOLDER
            // =====================================================

            var uploadsFolder = Path.Combine(
                _environment.WebRootPath,
                "uploads",
                "tenants",
                "agreements");

            Directory.CreateDirectory(uploadsFolder);


            // =====================================================
            // VALIDATION
            // =====================================================

            var allowedExtensions = new[]
            {
                ".jpg",
                ".jpeg",
                ".png"
            };

            const long maxFileSize =
                10 * 1024 * 1024; // 10 MB per image

            var uploadedCount = 0;


            // =====================================================
            // PROCESS EACH IMAGE
            // =====================================================

            foreach (var image in RentalAgreementImages)
            {
                if (image == null || image.Length == 0)
                {
                    continue;
                }

                if (image.Length > maxFileSize)
                {
                    continue;
                }

                var extension = Path
                    .GetExtension(image.FileName)
                    .ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    continue;
                }


                // Generate unique filename
                var fileName =
                    $"agreement_{tenant.TenantID}_{Guid.NewGuid():N}{extension}";

                var filePath = Path.Combine(
                    uploadsFolder,
                    fileName);


                // Save image
                await using (var stream = new FileStream(
                    filePath,
                    FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }


                // Save database record
                _context.RentalAgreementImages.Add(
                    new RentalAgreementImage
                    {
                        TenantID = tenant.TenantID,

                        ImagePath =
                            $"/uploads/tenants/agreements/{fileName}",

                        FileName =
                            Path.GetFileName(image.FileName),

                        UploadDate =
                            DateTime.Now
                    });

                uploadedCount++;
            }


            // =====================================================
            // NOTHING WAS UPLOADED
            // =====================================================

            if (uploadedCount == 0)
            {
                TempData["ErrorMessage"] =
                    "No valid rental agreement images were uploaded.";

                return RedirectToPage(
                    new { id = id.Value });
            }


            // =====================================================
            // SAVE
            // =====================================================

            await _context.SaveChangesAsync();


            // =====================================================
            // SUCCESS
            // =====================================================

            TempData["SuccessMessage"] =
                $"{uploadedCount} rental agreement image(s) uploaded successfully.";

            return RedirectToPage(
                new { id = tenant.TenantID });
        }
    }
}