using System.ComponentModel.DataAnnotations;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.MVVM.Models.PaymentModels
{
    public class RentalPayment
    {

        [Key]
        public int PaymentID { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Please select a tenant.")]
        [Display(Name = "Tenant")]
        public int TenantID { get; set; }

        public Tenant? Tenant { get; set; }

        [Required(ErrorMessage = "Amount is required.")]
        [Range(1, 1000000,
            ErrorMessage = "Amount must be greater than 0.")]
        [Display(Name = "Amount Paid")]
        [DataType(DataType.Currency)]
        public decimal AmountPaid { get; set; }

        [Required]
        [Display(Name = "Payment Date")]
        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Method")]
        public string PaymentMethod { get; set; }
            = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment Status")]
        public string PaymentStatus { get; set; }
            = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Payment For")]
        public string PaymentFor { get; set; }
            = string.Empty;

        [StringLength(200)]
        [Display(Name = "Remarks")]
        [DataType(DataType.MultilineText)]
        public string? Remarks { get; set; }

        [Display(Name = "Receipt Number")]
        [StringLength(30)]
        public string ReceiptNo { get; set; }
            = string.Empty;

        [Required]
        [Display(Name = "Billing Month")]
        [DataType(DataType.Date)]
        public DateTime BillingMonth { get; set; }

        public int? BillingStatementID { get; set; }

        public BillingStatement? BillingStatement { get; set; }

        public decimal AppliedAmount { get; set; }
    }
}
