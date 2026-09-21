using ARMTIS_Capstone_Project.MVVM.Models;
using ARMTIS_Capstone_Project.MVVM.Models.Identity;
using ARMTIS_Capstone_Project.MVVM.Models.Main;
using ARMTIS_Capstone_Project.MVVM.Models.PaymentModels;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Security.Claims;

namespace ARMTIS_Capstone_Project.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private bool _isWritingAuditLog;

        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options,
            IHttpContextAccessor httpContextAccessor)
            : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<UtilityRate> UtilityRates { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Unit> Units { get; set; }
        public DbSet<BillingStatement> BillingStatements { get; set; }
        public DbSet<RentalPayment> RentalPayments { get; set; }
        public DbSet<UtilityReading> UtilityReadings { get; set; }
        public DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }
        public DbSet<TenantCredit> TenantCredits { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<RentalAgreementImage> RentalAgreementImages { get; set; }
        public override int SaveChanges()
        {
            if (_isWritingAuditLog)
            {
                return base.SaveChanges();
            }

            var pendingAudits = PrepareAuditEntries();
            var result = base.SaveChanges();

            if (pendingAudits.Count > 0)
            {
                AddAuditRecords(pendingAudits);
                _isWritingAuditLog = true;
                try
                {
                    base.SaveChanges();
                }
                finally
                {
                    _isWritingAuditLog = false;
                }
            }

            return result;
        }

        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            if (_isWritingAuditLog)
            {
                return await base.SaveChangesAsync(cancellationToken);
            }

            var pendingAudits = PrepareAuditEntries();
            var result = await base.SaveChangesAsync(cancellationToken);

            if (pendingAudits.Count > 0)
            {
                AddAuditRecords(pendingAudits);
                _isWritingAuditLog = true;
                try
                {
                    await base.SaveChangesAsync(cancellationToken);
                }
                finally
                {
                    _isWritingAuditLog = false;
                }
            }

            return result;
        }

        private List<(EntityEntry Entry, AuditLog Audit)> PrepareAuditEntries()
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var user = httpContext?.User;

            if (user?.Identity?.IsAuthenticated != true)
            {
                return new List<(EntityEntry, AuditLog)>();
            }

            var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
            var userName = user.Identity?.Name;
            var userRole = user.FindAll(ClaimTypes.Role)
                .Select(c => c.Value)
                .FirstOrDefault();

            var now = DateTime.Now;
            var requestMethod = httpContext?.Request.Method;
            var requestPath = httpContext?.Request.Path.Value;
            var ipAddress = httpContext?.Connection.RemoteIpAddress?.ToString();

            var entries = ChangeTracker.Entries()
                .Where(e => ShouldAuditEntity(e.Entity)
                    && e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
                .ToList();

            var audits = new List<(EntityEntry, AuditLog)>();

            foreach (var entry in entries)
            {
                var entityName = entry.Metadata.ClrType.Name;

                if (entry.State == EntityState.Modified)
                {
                    var changedProperties = entry.Properties
                        .Where(p => p.IsModified && !IsSensitiveProperty(p.Metadata.Name))
                        .Select(p => p.Metadata.Name)
                        .ToList();

                    if (changedProperties.Count == 0)
                    {
                        continue;
                    }

                    var audit = CreateAudit(
                        userId,
                        userName,
                        userRole,
                        "Updated",
                        entityName,
                        null,
                        $"Updated {entityName} record. Changed: {string.Join(", ", changedProperties)}.",
                        now,
                        requestMethod,
                        requestPath,
                        ipAddress);

                    audits.Add((entry, audit));
                    continue;
                }

                var action = entry.State == EntityState.Added ? "Created" : "Deleted";
                var description = entry.State == EntityState.Added
                    ? $"Created {entityName} record."
                    : $"Deleted {entityName} record.";

                audits.Add((entry, CreateAudit(
                    userId,
                    userName,
                    userRole,
                    action,
                    entityName,
                    null,
                    description,
                    now,
                    requestMethod,
                    requestPath,
                    ipAddress)));
            }

            return audits;
        }

        private static bool ShouldAuditEntity(object entity)
        {
            if (entity is AuditLog || entity is Notification)
            {
                return false;
            }

            var name = entity.GetType().Name;

            return name is not "IdentityRole"
                and not "IdentityRoleClaim<string>"
                and not "IdentityUserClaim<string>"
                and not "IdentityUserLogin<string>"
                and not "IdentityUserRole<string>"
                and not "IdentityUserToken<string>";
        }

        private static bool IsSensitiveProperty(string propertyName)
        {
            return propertyName.Equals("PasswordHash", StringComparison.OrdinalIgnoreCase)
                || propertyName.Equals("SecurityStamp", StringComparison.OrdinalIgnoreCase)
                || propertyName.Equals("ConcurrencyStamp", StringComparison.OrdinalIgnoreCase);
        }

        private static AuditLog CreateAudit(
            string? userId,
            string? userName,
            string? userRole,
            string action,
            string entityName,
            string? entityId,
            string? description,
            DateTime timestamp,
            string? requestMethod,
            string? requestPath,
            string? ipAddress)
        {
            return new AuditLog
            {
                UserID = userId,
                UserName = userName,
                UserRole = userRole,
                Action = action,
                EntityName = entityName,
                EntityID = entityId,
                Description = description,
                Timestamp = timestamp,
                RequestMethod = requestMethod,
                RequestPath = requestPath,
                IpAddress = ipAddress
            };
        }

        private void AddAuditRecords(List<(EntityEntry Entry, AuditLog Audit)> pendingAudits)
        {
            foreach (var (entry, audit) in pendingAudits)
            {
                var primaryKey = entry.Metadata.FindPrimaryKey()?.Properties.FirstOrDefault();

                if (primaryKey != null)
                {
                    var keyValue = entry.Property(primaryKey.Name).CurrentValue;
                    audit.EntityID = keyValue?.ToString();
                }

                AuditLogs.Add(audit);
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ApplicationUser → Tenant
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Unit>()
                .Property(u => u.MonthlyRent)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Tenant>()
                .HasOne(t => t.Unit)
                .WithMany(u => u.Tenants)
                .HasForeignKey(t => t.UnitID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BillingStatement>()
                .HasOne(b => b.Unit)
                .WithMany()
                .HasForeignKey(b => b.UnitID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<BillingStatement>()
                .HasOne(b => b.Tenant)
                .WithMany()
                .HasForeignKey(b => b.TenantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<RentalPayment>()
                .HasOne(r => r.BillingStatement)
                .WithMany()
                .HasForeignKey(r => r.BillingStatementID)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<UtilityReading>()
                .HasOne(u => u.Tenant)
                .WithMany()
                .HasForeignKey(u => u.TenantID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UtilityReading>()
                .HasOne(u => u.Unit)
                .WithMany()
                .HasForeignKey(u => u.UnitID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<UtilityReading>()
                .HasIndex(u => new
                {
                    u.TenantID,
                    u.BillingMonth
                })
                .IsUnique();

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.Tenant)
                .WithMany()
                .HasForeignKey(n => n.TenantID)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Notification>()
                .HasOne(n => n.RecipientUser)
                .WithMany()
                .HasForeignKey(n => n.RecipientUserID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.TenantID);

            modelBuilder.Entity<Notification>()
                .HasIndex(n => n.RecipientUserID);
        }
    }
}
