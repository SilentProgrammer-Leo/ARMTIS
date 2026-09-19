using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace ARMTIS_Capstone_Project.MVVM.Models
{
    public class Notification
    {
        public int NotificationID { get; set; }

        public int? TenantID { get; set; }

        public string? RecipientUserID { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string? NotificationType { get; set; }

        public bool IsRead { get; set; } = false;

        public DateTime CreatedDate { get; set; }

        [ValidateNever]
        public Tenant? Tenant { get; set; }

        [ValidateNever]
        public ApplicationUser? RecipientUser { get; set; }
    }
}
