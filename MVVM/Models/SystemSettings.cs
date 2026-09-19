using System.ComponentModel.DataAnnotations;

namespace ARMTIS_Capstone_Project.MVVM.Models
{
    public class SystemSetting
    {
        [Key]
        public int SystemSettingID { get; set; }

        [Required]
        [StringLength(100)]
        [Display(Name = "Property Name")]
        public string PropertyName { get; set; } = "Tressing Residency";

        [Required]
        [StringLength(250)]
        [Display(Name = "Property Address")]
        public string PropertyAddress { get; set; } = string.Empty;

        [StringLength(30)]
        [Display(Name = "Contact Number")]
        public string? ContactNumber { get; set; }

        [EmailAddress]
        [StringLength(100)]
        [Display(Name = "Email Address")]
        public string? Email { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime DateUpdated { get; set; } = DateTime.Now;
    }
}