using System.ComponentModel.DataAnnotations;

namespace ARMTIS_Capstone_Project.MVVM.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogID { get; set; }

        [StringLength(450)]
        public string? UserID { get; set; }

        [StringLength(256)]
        public string? UserName { get; set; }

        [StringLength(100)]
        public string? UserRole { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [StringLength(100)]
        public string? EntityName { get; set; }

        [StringLength(100)]
        public string? EntityID { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public DateTime Timestamp { get; set; }

        [StringLength(45)]
        public string? IpAddress { get; set; }

        [StringLength(10)]
        public string? RequestMethod { get; set; }

        [StringLength(500)]
        public string? RequestPath { get; set; }

        public int? StatusCode { get; set; }
    }
}
