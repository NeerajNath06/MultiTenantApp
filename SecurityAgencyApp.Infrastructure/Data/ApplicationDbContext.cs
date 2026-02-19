using Microsoft.EntityFrameworkCore;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
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
    }
}
