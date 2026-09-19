using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.MVVM.Models.PaymentModels
{
    public class TenantCredit
    {
        public int TenantCreditID { get; set; }

        public int TenantID { get; set; }

        public decimal Amount { get; set; }

        public decimal RemainingAmount { get; set; }

        public DateTime DateCreated { get; set; }

        public string? SourceReceiptNo { get; set; }

        public string? Remarks { get; set; }

        public Tenant Tenant { get; set; } = null!;
    }
}
