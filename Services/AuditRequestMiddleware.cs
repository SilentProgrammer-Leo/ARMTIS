using System.Security.Claims;
using ARMTIS_Capstone_Project.Data;
using ARMTIS_Capstone_Project.MVVM.Models;

namespace ARMTIS_Capstone_Project.Services
{
    public class AuditRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public AuditRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext httpContext,
            ApplicationDbContext context)
        {
            Exception? exception = null;

            try
            {
                await _next(httpContext);
            }
            catch (Exception ex)
            {
                exception = ex;
                throw;
            }
            finally
            {
                await TryLogRequestAsync(httpContext, context, exception);
            }
        }

        private static async Task TryLogRequestAsync(
            HttpContext httpContext,
            ApplicationDbContext context,
            Exception? exception)
        {
            if (httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return;
            }

            var path = httpContext.Request.Path.Value ?? string.Empty;

            if (string.IsNullOrWhiteSpace(path)
                || path.StartsWith("/css/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/js/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/lib/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/images/", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/favicon.ico", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/_framework/", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Account/Login", StringComparison.OrdinalIgnoreCase)
                || path.StartsWith("/Account/Logout", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var action = httpContext.Request.Method switch
            {
                "GET" or "HEAD" => "Viewed",
                "POST" => "Submitted",
                "PUT" or "PATCH" => "Updated",
                "DELETE" => "Deleted",
                _ => httpContext.Request.Method
            };

            var role = httpContext.User.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .FirstOrDefault();

            var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            context.AuditLogs.Add(new AuditLog
            {
                UserID = userId,
                UserName = httpContext.User.Identity?.Name,
                UserRole = role,
                Action = action,
                Description = exception == null
                    ? $"{action} request to {path}."
                    : $"{action} request to {path} failed with status {httpContext.Response.StatusCode}.",
                Timestamp = DateTime.Now,
                IpAddress = httpContext.Connection.RemoteIpAddress?.ToString(),
                RequestMethod = httpContext.Request.Method,
                RequestPath = path,
                StatusCode = exception == null ? httpContext.Response.StatusCode : 500
            });

            await context.SaveChangesAsync();
        }
    }
}
