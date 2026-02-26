using Microsoft.EntityFrameworkCore;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Application.Interfaces;

namespace SecurityAgencyApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    private readonly ITenantContext? _tenantContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ITenantContext tenantContext) : base(options)
    {
        _tenantContext = tenantContext;
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
    public DbSet<Site> Sites { get; set; }
    public DbSet<SiteSupervisor> SiteSupervisors { get; set; }
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
        modelBuilder.Entity<Site>().HasQueryFilter(e => e.TenantId == _tenantContext!.TenantId);
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
}
