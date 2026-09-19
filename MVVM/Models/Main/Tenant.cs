using System.ComponentModel.DataAnnotations;


namespace ARMTIS_Capstone_Project.MVVM.Models.Main
{
    public class Tenant
    {
        [Key]
        public int TenantID { get; set; }


        [Required]
        public string FirstName { get; set; } = string.Empty;


        [Required]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string Address { get; set; } = string.Empty;

        [Required]
        [Phone]
        public string ContactNo { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [Required]
        public DateTime LeaseStartDate { get; set; }

        [Required]

        public DateTime LeaseEndDate { get; set; }


        // Foreign Key
        public int UnitID { get; set; }


        // For navigation
        public Unit? Unit { get; set; }

        
    }
}
