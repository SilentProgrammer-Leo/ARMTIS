namespace ARMTIS_Capstone_Project.MVVM.ViewModels
{
    public class UtilityReadingRow
    {
        public int TenantID { get; set; }

        public int UnitID { get; set; }

        public string TenantName { get; set; }
            = string.Empty;

        public string UnitNumber { get; set; }
            = string.Empty;

        public DateTime BillingMonth { get; set; }

        public decimal ElectricityBill { get; set; }

        public decimal WaterBill { get; set; }

        public decimal InternetBill { get; set; }

        public decimal TotalUtilityCharges { get; set; }

        public bool HasReading { get; set; }
    }
}
