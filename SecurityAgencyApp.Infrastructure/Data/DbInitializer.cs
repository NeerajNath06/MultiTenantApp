using Microsoft.EntityFrameworkCore;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // Ensure database is created
        await context.Database.EnsureCreatedAsync();

        // Check if data already exists
        if (context.Tenants.Any())
        {
            return; // Database already seeded
        }

        // Seed Tenant
        var tenant = new Tenant
        {
            CompanyName = "Demo Security Agency",
            RegistrationNumber = "REG001",
            Email = "admin@demoagency.com",
            Phone = "1234567890",
            Address = "123 Demo Street",
            City = "Demo City",
            State = "Demo State",
            Country = "India",
            PinCode = "123456",
            IsActive = true,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionEndDate = DateTime.UtcNow.AddYears(1)
        };
        context.Tenants.Add(tenant);
        await context.SaveChangesAsync();

        // Seed Permissions
        var permissions = new List<Permission>
        {
            new Permission { Resource = "Users", Action = "Create", Description = "Create users" },
            new Permission { Resource = "Users", Action = "Read", Description = "View users" },
            new Permission { Resource = "Users", Action = "Update", Description = "Update users" },
            new Permission { Resource = "Users", Action = "Delete", Description = "Delete users" },
            new Permission { Resource = "Departments", Action = "Create", Description = "Create departments" },
            new Permission { Resource = "Departments", Action = "Read", Description = "View departments" },
            new Permission { Resource = "Departments", Action = "Update", Description = "Update departments" },
            new Permission { Resource = "Departments", Action = "Delete", Description = "Delete departments" },
            new Permission { Resource = "Roles", Action = "Create", Description = "Create roles" },
            new Permission { Resource = "Roles", Action = "Read", Description = "View roles" },
            new Permission { Resource = "Roles", Action = "Update", Description = "Update roles" },
            new Permission { Resource = "Roles", Action = "Delete", Description = "Delete roles" },
            new Permission { Resource = "Menus", Action = "Create", Description = "Create menus" },
            new Permission { Resource = "Menus", Action = "Read", Description = "View menus" },
            new Permission { Resource = "Menus", Action = "Update", Description = "Update menus" },
            new Permission { Resource = "Menus", Action = "Delete", Description = "Delete menus" }
        };
        context.Permissions.AddRange(permissions);
        await context.SaveChangesAsync();

        // Seed Roles
        var adminRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Administrator",
            Code = "ADMIN",
            Description = "Full system access",
            IsSystemRole = true,
            IsActive = true
        };
        var guardRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Security Guard",
            Code = "GUARD",
            Description = "Security guard – can use mobile app",
            IsSystemRole = true,
            IsActive = true
        };
        var supervisorRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Supervisor",
            Code = "SUPERVISOR",
            Description = "Supervisor – can use mobile app",
            IsSystemRole = true,
            IsActive = true
        };
        context.Roles.Add(adminRole);
        context.Roles.Add(guardRole);
        context.Roles.Add(supervisorRole);
        await context.SaveChangesAsync();

        // Assign all permissions to admin role
        foreach (var permission in permissions)
        {
            context.RolePermissions.Add(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            });
        }
        await context.SaveChangesAsync();

        // Seed Department
        var department = new Department
        {
            TenantId = tenant.Id,
            Name = "Administration",
            Code = "ADMIN",
            Description = "Administration Department",
            IsActive = true
        };
        context.Departments.Add(department);
        await context.SaveChangesAsync();

        // Seed Admin User
        var adminPlainPassword = "Admin@123";
        var adminUser = new User
        {
            TenantId = tenant.Id,
            UserName = "admin",
            Email = "admin@demoagency.com",
            PasswordHash = passwordHasher.HashPassword(adminPlainPassword),
            Password = adminPlainPassword,
            FirstName = "System",
            LastName = "Administrator",
            PhoneNumber = "1234567890",
            DepartmentId = department.Id,
            IsActive = true
        };
        context.Users.Add(adminUser);
        await context.SaveChangesAsync();

        // Assign admin role to admin user
        context.UserRoles.Add(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        });
        await context.SaveChangesAsync();

        // Seed Menus – all sidebar items in DB (Web loads from API)
        var menus = new List<Menu>
        {
            new Menu { TenantId = tenant.Id, Name = "Dashboard", DisplayName = "Dashboard", Icon = "fas fa-home", Route = "Home", DisplayOrder = 1, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Users", DisplayName = "Users", Icon = "fas fa-users", Route = "Users", DisplayOrder = 2, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Departments", DisplayName = "Departments", Icon = "fas fa-building", Route = "Departments", DisplayOrder = 3, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Designations", DisplayName = "Designations", Icon = "fas fa-briefcase", Route = "Designations", DisplayOrder = 4, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Roles", DisplayName = "Roles", Icon = "fas fa-user-tag", Route = "Roles", DisplayOrder = 5, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Menus", DisplayName = "Menus", Icon = "fas fa-list", Route = "Menus", DisplayOrder = 6, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "SubMenus", DisplayName = "Sub Menus", Icon = "fas fa-list-ul", Route = "SubMenus", DisplayOrder = 7, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "SecurityGuards", DisplayName = "Security Guards", Icon = "fas fa-user-shield", Route = "SecurityGuards", DisplayOrder = 8, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Sites", DisplayName = "Sites", Icon = "fas fa-building", Route = "Sites", DisplayOrder = 9, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "GuardAssignments", DisplayName = "Assignments", Icon = "fas fa-user-check", Route = "GuardAssignments", DisplayOrder = 10, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Attendance", DisplayName = "Attendance", Icon = "fas fa-calendar-check", Route = "Attendance", DisplayOrder = 11, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Incidents", DisplayName = "Incidents", Icon = "fas fa-exclamation-triangle", Route = "Incidents", DisplayOrder = 12, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Shifts", DisplayName = "Shifts", Icon = "fas fa-clock", Route = "Shifts", DisplayOrder = 13, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "FormBuilder", DisplayName = "Form Builder", Icon = "fas fa-file-alt", Route = "FormBuilder", DisplayOrder = 14, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Bills", DisplayName = "Bills", Icon = "fas fa-file-invoice", Route = "Bills", DisplayOrder = 15, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Wages", DisplayName = "Wages", Icon = "fas fa-money-bill-wave", Route = "Wages", DisplayOrder = 16, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Clients", DisplayName = "Clients", Icon = "fas fa-building", Route = "Clients", DisplayOrder = 17, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Contracts", DisplayName = "Contracts", Icon = "fas fa-file-contract", Route = "Contracts", DisplayOrder = 18, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Payments", DisplayName = "Payments", Icon = "fas fa-money-check", Route = "Payments", DisplayOrder = 19, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "LeaveRequests", DisplayName = "Leave Requests", Icon = "fas fa-calendar-times", Route = "LeaveRequests", DisplayOrder = 20, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Expenses", DisplayName = "Expenses", Icon = "fas fa-receipt", Route = "Expenses", DisplayOrder = 21, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "TrainingRecords", DisplayName = "Training", Icon = "fas fa-graduation-cap", Route = "TrainingRecords", DisplayOrder = 22, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Equipment", DisplayName = "Equipment", Icon = "fas fa-tools", Route = "Equipment", DisplayOrder = 23, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Visitors", DisplayName = "Visitors", Icon = "fas fa-user-friends", Route = "Visitors", DisplayOrder = 24, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Compliance", DisplayName = "Compliance", Icon = "fas fa-clipboard-check", Route = "Compliance", DisplayOrder = 25, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Announcements", DisplayName = "Announcements", Icon = "fas fa-bullhorn", Route = "Announcements", DisplayOrder = 26, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "PatrolScans", DisplayName = "Patrol Scans", Icon = "fas fa-qrcode", Route = "PatrolScans", DisplayOrder = 27, IsActive = true },
            new Menu { TenantId = tenant.Id, Name = "Notifications", DisplayName = "Notifications", Icon = "fas fa-bell", Route = "Notifications", DisplayOrder = 28, IsActive = true }
        };
        context.Menus.AddRange(menus);
        await context.SaveChangesAsync();

        // Assign menus to admin role
        foreach (var menu in menus)
        {
            context.RoleMenus.Add(new RoleMenu
            {
                RoleId = adminRole.Id,
                MenuId = menu.Id
            });
        }
        await context.SaveChangesAsync();
    }

    /// <summary>
    /// For existing databases: add any new menus that were added to the app but not yet in DB.
    /// Call this on startup after SeedAsync so existing tenants get Visitors, Compliance, Announcements, PatrolScans, Notifications.
    /// </summary>
    public static async Task EnsureNewMenusAsync(ApplicationDbContext context)
    {
        if (!context.Tenants.Any())
            return;

        var newMenusDef = new[]
        {
            ("Visitors", "Visitors", "fas fa-user-friends", "Visitors", 24),
            ("Compliance", "Compliance", "fas fa-clipboard-check", "Compliance", 25),
            ("Announcements", "Announcements", "fas fa-bullhorn", "Announcements", 26),
            ("PatrolScans", "Patrol Scans", "fas fa-qrcode", "PatrolScans", 27),
            ("Notifications", "Notifications", "fas fa-bell", "Notifications", 28)
        };

        foreach (var tenant in context.Tenants.Where(t => t.IsActive))
        {
            var existingNames = await context.Menus
                .Where(m => m.TenantId == tenant.Id && newMenusDef.Select(x => x.Item1).Contains(m.Name))
                .Select(m => m.Name)
                .ToListAsync();

            var adminRole = await context.Roles.FirstOrDefaultAsync(r => r.TenantId == tenant.Id && r.Code == "ADMIN");
            if (adminRole == null)
                continue;

            foreach (var (name, displayName, icon, route, displayOrder) in newMenusDef)
            {
                if (existingNames.Contains(name))
                    continue;

                var menu = new Menu
                {
                    TenantId = tenant.Id,
                    Name = name,
                    DisplayName = displayName,
                    Icon = icon,
                    Route = route,
                    DisplayOrder = displayOrder,
                    IsActive = true
                };
                context.Menus.Add(menu);
                await context.SaveChangesAsync();

                context.RoleMenus.Add(new RoleMenu { RoleId = adminRole.Id, MenuId = menu.Id });
                await context.SaveChangesAsync();
            }
        }
    }
}
