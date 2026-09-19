using System.Security.Claims;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;

namespace ARMTIS_Capstone_Project.Services
{
    public class AuditLogService
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuditLogService(
            ApplicationDbContext context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task LogAsync(
            string action,
            string description,
            string? userId = null,
            string? userName = null,
            string? userRole = null)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var principal = httpContext?.User;

            userId ??= principal?.FindFirstValue(ClaimTypes.NameIdentifier);
            userName ??= principal?.Identity?.Name;
            userRole ??= principal?.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .FirstOrDefault();

            _context.AuditLogs.Add(new AuditLog
            {
                UserID = userId,
                UserName = userName,
                UserRole = userRole,
                Action = action,
                Description = description,
                Timestamp = DateTime.Now,
                IpAddress = httpContext?.Connection.RemoteIpAddress?.ToString(),
                RequestMethod = httpContext?.Request.Method,
                RequestPath = httpContext?.Request.Path.Value,
                StatusCode = httpContext?.Response.StatusCode
            });

            await _context.SaveChangesAsync();
        }
    }
}
