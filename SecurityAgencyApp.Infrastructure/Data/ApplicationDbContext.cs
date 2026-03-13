using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Application.Interfaces;
using System.Text.Json;

namespace SecurityAgencyApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;
    private readonly ICurrentUserService? _currentUserService;
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantContext? tenantContext = null,
        ICurrentUserService? currentUserService = null,
        IHttpContextAccessor? httpContextAccessor = null) : base(options)
    {
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _httpContextAccessor = httpContextAccessor;
    }

    // Core Entities
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Department> Departments { get; set; }
    public DbSet<Designation> Designations { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserPermission> UserPermissions { get; set; }

    // Menu/SubMenu Entities
    public DbSet<Menu> Menus { get; set; }
    public DbSet<SubMenu> SubMenus { get; set; }
    public DbSet<RoleMenu> RoleMenus { get; set; }
    public DbSet<RoleSubMenu> RoleSubMenus { get; set; }
    public DbSet<UserMenu> UserMenus { get; set; }
    public DbSet<UserSubMenu> UserSubMenus { get; set; }
    public DbSet<MenuPermission> MenuPermissions { get; set; }
    public DbSet<SubMenuPermission> SubMenuPermissions { get; set; }

    // Security Guard Entities
    public DbSet<SecurityGuard> SecurityGuards { get; set; }
    public DbSet<GuardDocument> GuardDocuments { get; set; }
    public DbSet<TenantDocument> TenantDocuments { get; set; }
    public DbSet<GuardAssignment> GuardAssignments { get; set; }
    public DbSet<Branch> Branches { get; set; }
    public DbSet<Site> Sites { get; set; }
    public DbSet<SitePost> SitePosts { get; set; }
    public DbSet<SiteDeploymentPlan> SiteDeploymentPlans { get; set; }
    public DbSet<SiteSupervisor> SiteSupervisors { get; set; }
    public DbSet<SiteRatePlan> SiteRatePlans { get; set; }
    public DbSet<Shift> Shifts { get; set; }

    // Form Builder Entities
    public DbSet<FormTemplate> FormTemplates { get; set; }
    public DbSet<FormField> FormFields { get; set; }
    public DbSet<FormSubmission> FormSubmissions { get; set; }
    public DbSet<FormSubmissionData> FormSubmissionData { get; set; }

    // Operations Entities
    public DbSet<GuardAttendance> GuardAttendances { get; set; }
    public DbSet<IncidentReport> IncidentReports { get; set; }
    public DbSet<IncidentAttachment> IncidentAttachments { get; set; }

    // Bill & Wages Entities
    public DbSet<Bill> Bills { get; set; }
    public DbSet<BillItem> BillItems { get; set; }
    public DbSet<Wage> Wages { get; set; }
    public DbSet<WageDetail> WageDetails { get; set; }
    public DbSet<SalaryStructure> SalaryStructures { get; set; }
    public DbSet<PayrollRun> PayrollRuns { get; set; }

    // Client & Contract Entities
    public DbSet<Client> Clients { get; set; }
    public DbSet<Contract> Contracts { get; set; }
    public DbSet<ContractSite> ContractSites { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // Operations Entities
    public DbSet<LeaveRequest> LeaveRequests { get; set; }
    public DbSet<Expense> Expenses { get; set; }
    public DbSet<TrainingRecord> TrainingRecords { get; set; }
    public DbSet<Equipment> Equipment { get; set; }
    public DbSet<Announcement> Announcements { get; set; }
    public DbSet<Visitor> Visitors { get; set; }
    public DbSet<VehicleLog> VehicleLogs { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<PatrolScan> PatrolScans { get; set; }

    // Audit
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Configure enums as strings
        modelBuilder.Entity<GuardAssignment>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<GuardAttendance>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<FormField>()
            .Property(e => e.FieldType)
            .HasConversion<string>();

        modelBuilder.Entity<FormSubmission>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<IncidentReport>()
            .Property(e => e.Severity)
            .HasConversion<string>();

        modelBuilder.Entity<IncidentReport>()
            .Property(e => e.Status)
            .HasConversion<string>();

        modelBuilder.Entity<SecurityGuard>()
            .Property(e => e.Gender)
            .HasConversion<string>();

        // Enterprise: composite indexes for tenant-scoped list/export (large-tenant scale)
        modelBuilder.Entity<IncidentReport>()
            .HasIndex(e => new { e.TenantId, e.IncidentDate });
        modelBuilder.Entity<GuardAttendance>()
            .HasIndex(e => new { e.TenantId, e.AttendanceDate });
        modelBuilder.Entity<GuardAssignment>()
            .HasIndex(e => new { e.TenantId, e.AssignmentStartDate });
        modelBuilder.Entity<VehicleLog>()
            .HasIndex(e => new { e.TenantId, e.EntryTime });

        // Global query filter: tenant-scoped entities only return data for current tenant (enterprise safety).
        if (_tenantContext != null)
            ApplyTenantGlobalFilters(modelBuilder);
    }

    private void ApplyTenantGlobalFilters(ModelBuilder modelBuilder)
    {
        // Use _tenantContext.TenantId so filter is evaluated at query time (per request).
        modelBuilder.Entity<User>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Department>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Designation>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Role>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Menu>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SubMenu>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Branch>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Site>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SitePost>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SiteDeploymentPlan>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SiteSupervisor>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Shift>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SecurityGuard>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<GuardAssignment>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<GuardAttendance>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<IncidentReport>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Client>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Contract>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Payment>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Bill>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Wage>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SalaryStructure>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<PayrollRun>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<SiteRatePlan>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<LeaveRequest>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Expense>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<TrainingRecord>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Equipment>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Announcement>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Visitor>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<VehicleLog>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<Notification>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<PatrolScan>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
        modelBuilder.Entity<FormSubmission>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var utcNow = DateTime.UtcNow;
        var auditLogs = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.Entity is AuditLog || entry.State == EntityState.Detached || entry.State == EntityState.Unchanged)
                continue;

            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedDate = utcNow;
                entry.Entity.CreatedBy ??= _currentUserService?.UserId;
            }
            else
            {
                entry.Entity.ModifiedDate = utcNow;
                entry.Entity.ModifiedBy = _currentUserService?.UserId;
            }

            var auditLog = CreateAuditLog(entry, utcNow);
            if (auditLog != null)
                auditLogs.Add(auditLog);
        }

        if (auditLogs.Count > 0)
            AuditLogs.AddRange(auditLogs);

        return await base.SaveChangesAsync(cancellationToken);
    }

    private AuditLog? CreateAuditLog(Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry<BaseEntity> entry, DateTime utcNow)
    {
        if (entry.Entity is AuditLog)
            return null;

        var action = entry.State switch
        {
            EntityState.Added => "Create",
            EntityState.Modified => "Update",
            EntityState.Deleted => "Delete",
            _ => null
        };

        if (action == null)
            return null;

        var oldValues = new Dictionary<string, object?>();
        var newValues = new Dictionary<string, object?>();

        foreach (var property in entry.Properties)
        {
            if (property.Metadata.IsPrimaryKey())
                continue;

            if (entry.State == EntityState.Added)
            {
                newValues[property.Metadata.Name] = property.CurrentValue;
                continue;
            }

            if (entry.State == EntityState.Deleted)
            {
                oldValues[property.Metadata.Name] = property.OriginalValue;
                continue;
            }

            if (!property.IsModified)
                continue;

            oldValues[property.Metadata.Name] = property.OriginalValue;
            newValues[property.Metadata.Name] = property.CurrentValue;
        }

        if (entry.State == EntityState.Modified && oldValues.Count == 0 && newValues.Count == 0)
            return null;

        var httpContext = _httpContextAccessor?.HttpContext;
        var tenantId = entry.Entity is TenantEntity tenantEntity
            ? tenantEntity.TenantId
            : _tenantContext?.TenantId;

        return new AuditLog
        {
            TenantId = tenantId,
            UserId = _currentUserService?.UserId,
            Action = action,
            EntityType = entry.Metadata.ClrType.Name,
            EntityId = entry.Entity.Id.ToString(),
            OldValues = oldValues.Count > 0 ? JsonSerializer.Serialize(oldValues) : null,
            NewValues = newValues.Count > 0 ? JsonSerializer.Serialize(newValues) : null,
            IpAddress = httpContext?.Connection?.RemoteIpAddress?.ToString(),
            UserAgent = httpContext?.Request?.Headers.UserAgent.ToString(),
            CreatedDate = utcNow,
            CreatedBy = _currentUserService?.UserId
        };
    }
}
