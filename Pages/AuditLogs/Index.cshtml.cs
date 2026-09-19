using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ARMTIS_Capstone_Project.Pages.AuditLogs
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IList<AuditLog> Logs { get; set; } = new List<AuditLog>();

        public int TotalCount { get; set; }
        public int TodayCount { get; set; }
        public int DataChangeCount { get; set; }
        public int LoginCount { get; set; }

        public string? Search { get; set; }
        public string? ActionFilter { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        public async Task OnGetAsync(
            string? search,
            string? actionFilter,
            DateTime? fromDate,
            DateTime? toDate)
        {
            Search = search?.Trim();
            ActionFilter = actionFilter;
            FromDate = fromDate;
            ToDate = toDate;

            var query = _context.AuditLogs.AsNoTracking().AsQueryable();

            TotalCount = await query.CountAsync();

            var today = DateTime.Today;
            TodayCount = await query.CountAsync(l => l.Timestamp >= today);
            DataChangeCount = await query.CountAsync(l =>
                l.Action == "Created" ||
                l.Action == "Updated" ||
                l.Action == "Deleted");
            LoginCount = await query.CountAsync(l =>
                l.Action == "Login" || l.Action == "Login Failed" || l.Action == "Login Blocked");

            if (!string.IsNullOrWhiteSpace(Search))
            {
                query = query.Where(l =>
                    (l.UserName ?? "").Contains(Search) ||
                    (l.Description ?? "").Contains(Search) ||
                    (l.EntityName ?? "").Contains(Search) ||
                    (l.EntityID ?? "").Contains(Search) ||
                    (l.RequestPath ?? "").Contains(Search));
            }

            if (!string.IsNullOrWhiteSpace(ActionFilter) && ActionFilter != "All")
            {
                query = query.Where(l => l.Action == ActionFilter);
            }

            if (FromDate.HasValue)
            {
                query = query.Where(l => l.Timestamp >= FromDate.Value.Date);
            }

            if (ToDate.HasValue)
            {
                var endDate = ToDate.Value.Date.AddDays(1);
                query = query.Where(l => l.Timestamp < endDate);
            }

            Logs = await query
                .OrderByDescending(l => l.Timestamp)
                .Take(500)
                .ToListAsync();
        }
    }
}
