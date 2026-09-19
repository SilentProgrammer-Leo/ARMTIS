using System.ComponentModel.DataAnnotations;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.MVVM.Models.PaymentModels
{
    public class BillingStatement
    {
        [Key]
        public int BillingStatementID { get; set; }

        // Foreign Key
        [Required(ErrorMessage = "Please select a tenant.")]
        [Display(Name = "Tenant")]
        public int TenantID { get; set; }

        public Tenant? Tenant { get; set; }

        public int UnitID { get; set; }

        public Unit? Unit { get; set; }

        [Required]
        [Display(Name = "Billing Number")]
        [StringLength(30)]
        public string BillingNo { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Billing Month")]
        [DataType(DataType.Date)]
        public DateTime BillingMonth { get; set; }

        [Required]
        [Display(Name = "Due Date")]
        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Required]
        [Display(Name = "Monthly Rent")]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000)]
        public decimal MonthlyRent { get; set; }

        [Display(Name = "Penalty Amount")]
        [DataType(DataType.Currency)]
        [Range(0, 1000000)]
        public decimal PenaltyAmount { get; set; }

        [Display(Name = "Utility Charges")]
        [DataType(DataType.Currency)]
        [Range(0, 1000000)]
        public decimal UtilityCharges { get; set; }

        [Required]
        [Display(Name = "Total Amount Due")]
        [DataType(DataType.Currency)]
        [Range(0.01, 1000000)]
        public decimal TotalAmountDue { get; set; }

        [Required]
        [Display(Name = "Billing Status")]
        [StringLength(30)]
        public string BillingStatus { get; set; } = string.Empty;

        [Display(Name = "Remarks")]
        [StringLength(255)]
        [DataType(DataType.MultilineText)]
        public string? Remarks { get; set; }

        [Display(Name = "Date Generated")]
        [DataType(DataType.Date)]
        public DateTime DateGenerated { get; set; }
            = DateTime.Now;

        //added new for utilities
        public decimal Electricity { get; set; }

        public decimal Water { get; set; }

        public decimal Internet { get; set; }

        public decimal CreditApplied { get; set; } = 0;

    }
}
