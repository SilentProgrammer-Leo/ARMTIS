namespace ARMTIS_Capstone_Project.MVVM.Models.Main
{
    public class RentalAgreementImage
    {
        public int RentalAgreementImageID { get; set; }

        public int TenantID { get; set; }

        public Tenant? Tenant { get; set; }

        public string ImagePath { get; set; } = string.Empty;

        public string? FileName { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.Now;
    }
}
