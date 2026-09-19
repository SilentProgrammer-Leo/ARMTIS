using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace ARMTIS_Capstone_Project.MVVM.Models
{
    public class MaintenanceRequest
    {
        public int MaintenanceRequestID { get; set; }

        public int TenantID { get; set; }
        public int UnitID { get; set; }

        public string RequestTitle { get; set; } = string.Empty;

        public string? Category { get; set; }

        public string Description { get; set; } = string.Empty;

        public string Priority { get; set; } = "Medium";

        public string Status { get; set; } = "Pending";

        public DateTime RequestDate { get; set; }

        public DateTime? CompletedDate { get; set; }

        public string? AdminRemarks { get; set; }

        [ValidateNever]
        public Tenant Tenant { get; set; } = null!;

        [ValidateNever]
        public Unit Unit { get; set; } = null!; 
    }
}
