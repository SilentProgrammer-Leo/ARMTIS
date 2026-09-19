using System.ComponentModel.DataAnnotations;

namespace ARMTIS_Capstone_Project.MVVM.Models.Main
{
    public class Unit
    {
        [Key]
        public int UnitID { get; set; }

        [Required]
        public string UnitNumber { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        [Required]
        public decimal MonthlyRent { get; set; }

        [Required]
        public string UnitStatus { get; set; } = string.Empty;

        [Required]
        [Range(1, 3, ErrorMessage = "Capacity must be between 1 and 3.")]
        public int Capacity { get; set;  } 

        // One Unit can have many tenants (maximum 2)
        public ICollection<Tenant> Tenants { get; set; } = new List<Tenant>();
    }
}
