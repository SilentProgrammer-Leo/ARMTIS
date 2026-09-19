using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.Maintenance
{
    public class MaintenanceIndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public MaintenanceIndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<MaintenanceRequest> MaintenanceRequests { get; set; }
            = new List<MaintenanceRequest>();

        public int TotalRequests { get; set; }
        public int PendingRequests { get; set; }
        public int InProgressRequests { get; set; }
        public int CompletedRequests { get; set; }

        public async Task OnGetAsync()
        {
            MaintenanceRequests = await _context.MaintenanceRequests
                .Include(m => m.Tenant)
                .Include(m => m.Unit)
                .OrderByDescending(m => m.RequestDate)
                .ThenByDescending(m => m.MaintenanceRequestID)
                .ToListAsync();

            TotalRequests = MaintenanceRequests.Count;

            PendingRequests = MaintenanceRequests.Count(
                m => m.Status == "Pending");

            InProgressRequests = MaintenanceRequests.Count(
                m => m.Status == "In Progress");

            CompletedRequests = MaintenanceRequests.Count(
                m => m.Status == "Completed");
        }
    }
}