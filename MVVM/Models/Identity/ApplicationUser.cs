
using Microsoft.AspNetCore.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;

namespace ARMTIS_Capstone_Project.MVVM.Models.Identity
{
    public class ApplicationUser : IdentityUser
    {
        public int? TenantID { get; set; }

        public Tenant? Tenant { get; set; }
    }
}
