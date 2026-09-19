using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARMTIS_Capstone_Project.MVVM.Models.PaymentModels
{
    public class UtilityRate
    {
        [Key]
        public int UtilityRateID { get; set; }

        [Required]
        [StringLength(30)]
        public string UtilityType { get; set; }
            = string.Empty;
        // Electricity / Water

        [Column(TypeName = "decimal(18,2)")]
        public decimal Rate { get; set; }

        [Required]
        public DateTime EffectiveFrom { get; set; }

        public DateTime? EffectiveTo { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime DateCreated { get; set; }
            = DateTime.Now;
    }
}
