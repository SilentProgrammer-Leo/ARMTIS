namespace ARMTIS_Capstone_Project.MVVM.ViewModels
{
    public class BillingGeneration
    {
        public int TenantID { get; set; }

        public int UnitID { get; set; }

        public string TenantName { get; set; } = "";

        public string UnitNumber { get; set; } = "";

        public decimal MonthlyRent { get; set; }

        public DateTime BillingMonth { get; set; }

        public DateTime DueDate { get; set; }

        // Entered through the utility modal
        public decimal Electricity { get; set; }
        public decimal Water { get; set; }
        public decimal Internet { get; set; }

        public decimal UtilityTotal { get; set; }

        public bool HasUtilityReading { get; set; }

        public bool BillingAlreadyExists { get; set; }
    }


}
