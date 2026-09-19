using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using ARMTIS_Capstone_Project.MVVM.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.UtilityReadingPages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }
    [BindProperty(SupportsGet = true)]
    public int? SelectedMonth { get; set; }

    [BindProperty(SupportsGet = true)]
    public int? SelectedYear { get; set; }

    public List<UtilityReadingRow> UtilityRows { get; set; }
        = new();

    // UTILITIES
    public IList<UtilityReading> UtilityReading { get; set; } = default!;

    public UtilityRate? CurrentElectricRate { get; set; }

    public UtilityRate? CurrentWaterRate { get; set; }
    public UtilityRate? CurrentInternetRate { get; set; }

    public class SaveUtilityReadingRequest
    {
        public int TenantID { get; set; }
        public int UnitID { get; set; }

        public int Month { get; set; }
        public int Year { get; set; }

        public decimal PreviousElectricReading { get; set; }
        public decimal CurrentElectricReading { get; set; }

        public decimal PreviousWaterReading { get; set; }
        public decimal CurrentWaterReading { get; set; }
        public decimal WaterRate { get; set; }

        public decimal InternetRate { get; set; }
        public decimal ElectricRate { get; set; }
    }


    public async Task<JsonResult> OnGetUtilityInfoAsync(
     int tenantId,
     int unitId,
     int month,
     int year)
    {
        var billingMonth =
            new DateTime(year, month, 1);

        var previousMonth =
            billingMonth.AddMonths(-1);

        // Previous meter reading
        var previousReading =
            await _context.UtilityReadings
                .FirstOrDefaultAsync(u =>
                    u.TenantID == tenantId &&
                    u.BillingMonth == previousMonth);


        // Electricity rate applicable to selected month
        var electricRate =
            await _context.UtilityRates
                .Where(r =>
                    r.UtilityType == "Electricity" &&
                    r.EffectiveFrom <= billingMonth &&
                    (
                        r.EffectiveTo == null ||
                        r.EffectiveTo >= billingMonth
                    ))
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefaultAsync();


        // Water rate applicable to selected month
        var waterRate =
            await _context.UtilityRates
                .Where(r =>
                    r.UtilityType == "Water" &&
                    r.EffectiveFrom <= billingMonth &&
                    (
                        r.EffectiveTo == null ||
                        r.EffectiveTo >= billingMonth
                    ))
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefaultAsync();


        // Internet rate applicable to selected month
        var internetRate =
            await _context.UtilityRates
                .Where(r =>
                    r.UtilityType == "Internet" &&
                    r.EffectiveFrom <= billingMonth &&
                    (
                        r.EffectiveTo == null ||
                        r.EffectiveTo >= billingMonth
                    ))
                .OrderByDescending(r => r.EffectiveFrom)
                .FirstOrDefaultAsync();


        return new JsonResult(new
        {
            hasPreviousReading =
                previousReading != null,

            previousElectricReading =
                previousReading?.CurrentElectricReading ?? 0,

            previousWaterReading =
                previousReading?.CurrentWaterReading ?? 0,

            electricRate =
                electricRate?.Rate ?? 0,

            waterRate =
                waterRate?.Rate ?? 0,

            internetRate =
                internetRate?.Rate ?? 0
        });
    }

    public async Task<IActionResult> OnPostSaveUtilityReadingAsync(
     [FromBody] SaveUtilityReadingRequest request)
    {

        var billingMonth =
            new DateTime(
                request.Year,
                request.Month,
                1);

        var existingReading =
            await _context.UtilityReadings
                .FirstOrDefaultAsync(u =>
                    u.TenantID == request.TenantID &&
                    u.BillingMonth == billingMonth);

        if (existingReading != null)
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "A utility reading already exists for this tenant and billing month."
            })
            {
                StatusCode = 400
            };
        }

        // VALIDATE READINGS
        if (request.CurrentElectricReading <
            request.PreviousElectricReading)
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "Current electricity reading cannot be lower than the previous reading."
            })
            {
                StatusCode = 400
            };
        }

        if (request.CurrentWaterReading <
            request.PreviousWaterReading)
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "Current water reading cannot be lower than the previous reading."
            })
            {
                StatusCode = 400
            };
        }

        // VALIDATE RATES
        if (request.ElectricRate <= 0)
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "Please enter a valid electricity rate."
            })
            {
                StatusCode = 400
            };
        }

        if (request.WaterRate <= 0)
        {
            return new JsonResult(new
            {
                success = false,
                message =
                    "Please enter a valid water rate."
            })
            {
                StatusCode = 400
            };
        }

        // ELECTRICITY
        decimal electricConsumption =
            request.CurrentElectricReading -
            request.PreviousElectricReading;

        decimal electricRate =
            request.ElectricRate;

        decimal electricityBill =
            electricConsumption *
            electricRate;

        // WATER
        decimal waterConsumption =
            request.CurrentWaterReading -
            request.PreviousWaterReading;

        decimal waterRate =
            request.WaterRate;

        decimal waterBill =
            waterConsumption *
            waterRate;

        // INTERNET
        decimal internetBill =
    request.InternetRate;

        // TOTAL
        decimal totalUtility =
            electricityBill +
            waterBill +
            internetBill;

        var utilityReading =
            new UtilityReading
            {
                TenantID =
                    request.TenantID,

                UnitID =
                    request.UnitID,

                BillingMonth =
                    billingMonth,

                PreviousElectricReading =
                    request.PreviousElectricReading,

                CurrentElectricReading =
                    request.CurrentElectricReading,

                ElectricConsumption =
                    electricConsumption,

                ElectricRate =
                    electricRate,

                ElectricityBill =
                    electricityBill,

                PreviousWaterReading =
                    request.PreviousWaterReading,

                CurrentWaterReading =
                    request.CurrentWaterReading,

                WaterConsumption =
                    waterConsumption,

                WaterRate =
                    waterRate,

                WaterBill =
                    waterBill,

                InternetBill =
                    internetBill,

                TotalUtilityCharges =
                    totalUtility,

                DateRecorded =
                    DateTime.Now
            };

        _context.UtilityReadings.Add(
            utilityReading);

        await _context.SaveChangesAsync();

        return new JsonResult(new
        {
            success = true,
            message =
                "Utility reading saved successfully."
        });
    }


    public async Task OnGetAsync()
    {
        // GET CURRENT ACTIVE UTILITY RATES

        CurrentElectricRate =
            await _context.UtilityRates
                .FirstOrDefaultAsync(r =>
                    r.UtilityType == "Electricity" &&
                    r.IsActive);

        CurrentWaterRate =
            await _context.UtilityRates
                .FirstOrDefaultAsync(r =>
                    r.UtilityType == "Water" &&
                    r.IsActive);

        CurrentInternetRate =
            await _context.UtilityRates
                .FirstOrDefaultAsync(r =>
                    r.UtilityType == "Internet" &&
                    r.IsActive);

        UtilityRows = new List<UtilityReadingRow>();

        if (!SelectedMonth.HasValue ||
            !SelectedYear.HasValue)
        {
            return;
        }

        var billingMonth =
            new DateTime(
                SelectedYear.Value,
                SelectedMonth.Value,
                1);

        var tenants = await _context.Tenants
            .Include(t => t.Unit)
            .ToListAsync();

        foreach (var tenant in tenants)
        {
            var reading =
                await _context.UtilityReadings
                    .FirstOrDefaultAsync(u =>
                        u.TenantID == tenant.TenantID &&
                        u.BillingMonth == billingMonth);

            UtilityRows.Add(
                new UtilityReadingRow
                {
                    TenantID = tenant.TenantID,

                    UnitID = tenant.UnitID,

                    TenantName =
                        tenant.FirstName + " " +
                        tenant.LastName,

                    UnitNumber =
                        tenant.Unit?.UnitNumber ?? "N/A",

                    BillingMonth = billingMonth,

                    ElectricityBill =
                        reading?.ElectricityBill ?? 0,

                    WaterBill =
                        reading?.WaterBill ?? 0,

                    InternetBill =
                        reading?.InternetBill ?? 300,

                    TotalUtilityCharges =
                        reading?.TotalUtilityCharges ?? 0,

                    HasReading =
                        reading != null
                });
        }



    }

}
