using ARMTIS_Capstone_Project.MVVM.Models.Main;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ARMTIS_Capstone_Project.MVVM.Models.PaymentModels
{
    public class UtilityReading
    {
        
            [Key]
            public int UtilityReadingID { get; set; }

            // Tenant
            [Required]
            public int TenantID { get; set; }

            public Tenant? Tenant { get; set; }

            // Unit
            [Required]
            public int UnitID { get; set; }

            public Unit? Unit { get; set; }

            // Billing Period
            [Required]
            [DataType(DataType.Date)]
            public DateTime BillingMonth { get; set; }


            // ELECTRICITY

            [Column(TypeName = "decimal(18,2)")]
            public decimal PreviousElectricReading { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal CurrentElectricReading { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal ElectricConsumption { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal ElectricRate { get; set; } 

        [Column(TypeName = "decimal(18,2)")]
            public decimal ElectricityBill { get; set; }

     
            // WATER
            
            [Column(TypeName = "decimal(18,2)")]
            public decimal PreviousWaterReading { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal CurrentWaterReading { get; set; }

            [Column(TypeName = "decimal(18,2)")]
            public decimal WaterConsumption { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal WaterRate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
            public decimal WaterBill { get; set; }


            // INTERNET

            [Column(TypeName = "decimal(18,2)")]
            public decimal InternetBill { get; set; } = 300.00m;


            // TOTAL

            [Column(TypeName = "decimal(18,2)")]
            public decimal TotalUtilityCharges { get; set; }

            public DateTime DateRecorded { get; set; }
                = DateTime.Now;

         }
}
