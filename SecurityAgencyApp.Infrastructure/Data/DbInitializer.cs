using Microsoft.EntityFrameworkCore;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Infrastructure.Data;

public static class DbInitializer
{
    /// <returns>True if seed ran and data was inserted; false if skipped (data already exists).</returns>
    public static async Task<bool> SeedAsync(ApplicationDbContext context, IPasswordHasher passwordHasher)
    {
        // Schema is applied by Program.cs via MigrateAsync(); do not use EnsureCreated here.

        // Check if data already exists – by count and by RegistrationNumber to avoid duplicate key
        if (await context.Tenants.AnyAsync())
        {
            return false; // Database already seeded
        }

        // Double-check: tenant with REG001 might exist from a previous partial seed (avoid duplicate key)
        if (await context.Tenants.AnyAsync(t => t.RegistrationNumber == "REG001"))
        {
            return false;
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
        var accountsRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Accounts",
            Code = "ACCOUNTS",
            Description = "Accounts / Finance department",
            IsSystemRole = true,
            IsActive = true
        };
        context.Roles.Add(adminRole);
        context.Roles.Add(guardRole);
        context.Roles.Add(supervisorRole);
        context.Roles.Add(accountsRole);
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

        // Seed demo Guard user (so Guard login works after DB reset; password: Guard@123)
        var guardPlainPassword = "Guard@123";
        var demoGuard = new SecurityGuard
        {
            TenantId = tenant.Id,
            GuardCode = "GRD0001",
            FirstName = "Demo",
            LastName = "Guard",
            Email = "guard@demoagency.com",
            PhoneNumber = "9876543210",
            JoiningDate = DateTime.UtcNow,
            IsActive = true
        };
        context.SecurityGuards.Add(demoGuard);
        await context.SaveChangesAsync();

        var guardUser = new User
        {
            TenantId = tenant.Id,
            UserName = "guard",
            Email = "guard@demoagency.com",
            PasswordHash = passwordHasher.HashPassword(guardPlainPassword),
            Password = guardPlainPassword,
            FirstName = demoGuard.FirstName,
            LastName = demoGuard.LastName,
            PhoneNumber = demoGuard.PhoneNumber,
            IsActive = true
        };
        context.Users.Add(guardUser);
        await context.SaveChangesAsync();
        demoGuard.UserId = guardUser.Id;
        context.SecurityGuards.Update(demoGuard);
        context.UserRoles.Add(new UserRole { UserId = guardUser.Id, RoleId = guardRole.Id });
        await context.SaveChangesAsync();

        // Seed Menus: Main Menu + SubMenu hierarchy (department/role-wise from DB, no hardcoding)
        var menuDashboard = new Menu { TenantId = tenant.Id, Name = "Dashboard", DisplayName = "Dashboard", Icon = "fas fa-home", Route = "Home", DisplayOrder = 1, IsActive = true };
        var menuAdmin = new Menu { TenantId = tenant.Id, Name = "Administration", DisplayName = "Administration", Icon = "fas fa-cog", Route = "#", DisplayOrder = 2, IsActive = true };
        var menuOps = new Menu { TenantId = tenant.Id, Name = "Operations", DisplayName = "Operations", Icon = "fas fa-tasks", Route = "#", DisplayOrder = 3, IsActive = true };
        var menuFinance = new Menu { TenantId = tenant.Id, Name = "Finance", DisplayName = "Finance", Icon = "fas fa-money-bill-wave", Route = "#", DisplayOrder = 4, IsActive = true };
        var menuHr = new Menu { TenantId = tenant.Id, Name = "HR", DisplayName = "HR", Icon = "fas fa-users-cog", Route = "#", DisplayOrder = 5, IsActive = true };
        var menuMore = new Menu { TenantId = tenant.Id, Name = "More", DisplayName = "More", Icon = "fas fa-ellipsis-h", Route = "#", DisplayOrder = 6, IsActive = true };
        context.Menus.AddRange(menuDashboard, menuAdmin, menuOps, menuFinance, menuHr, menuMore);
        await context.SaveChangesAsync();

        var mainMenus = new[] { menuDashboard, menuAdmin, menuOps, menuFinance, menuHr, menuMore };

        // SubMenus under each main menu
        var subAdmin = new List<SubMenu>
        {
            new SubMenu { TenantId = tenant.Id, MenuId = menuAdmin.Id, Name = "Users", DisplayName = "Users", Icon = "fas fa-users", Route = "Users", DisplayOrder = 1, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuAdmin.Id, Name = "Departments", DisplayName = "Departments", Icon = "fas fa-building", Route = "Departments", DisplayOrder = 2, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuAdmin.Id, Name = "Designations", DisplayName = "Designations", Icon = "fas fa-briefcase", Route = "Designations", DisplayOrder = 3, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuAdmin.Id, Name = "Roles", DisplayName = "Roles", Icon = "fas fa-user-tag", Route = "Roles", DisplayOrder = 4, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuAdmin.Id, Name = "Menus", DisplayName = "Menus", Icon = "fas fa-list", Route = "Menus", DisplayOrder = 5, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuAdmin.Id, Name = "SubMenus", DisplayName = "Sub Menus", Icon = "fas fa-list-ul", Route = "SubMenus", DisplayOrder = 6, IsActive = true }
        };
        var subOps = new List<SubMenu>
        {
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "SecurityGuards", DisplayName = "Security Guards", Icon = "fas fa-user-shield", Route = "SecurityGuards", DisplayOrder = 1, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "Sites", DisplayName = "Sites", Icon = "fas fa-building", Route = "Sites", DisplayOrder = 2, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "GuardAssignments", DisplayName = "Assignments", Icon = "fas fa-user-check", Route = "GuardAssignments", DisplayOrder = 3, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "Attendance", DisplayName = "Attendance", Icon = "fas fa-calendar-check", Route = "Attendance", DisplayOrder = 4, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "Incidents", DisplayName = "Incidents", Icon = "fas fa-exclamation-triangle", Route = "Incidents", DisplayOrder = 5, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "Shifts", DisplayName = "Shifts", Icon = "fas fa-clock", Route = "Shifts", DisplayOrder = 6, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "Visitors", DisplayName = "Visitors", Icon = "fas fa-user-friends", Route = "Visitors", DisplayOrder = 7, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "PatrolScans", DisplayName = "Patrol Scans", Icon = "fas fa-qrcode", Route = "PatrolScans", DisplayOrder = 8, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "FormBuilder", DisplayName = "Form Builder", Icon = "fas fa-file-alt", Route = "FormBuilder", DisplayOrder = 9, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuOps.Id, Name = "Roster", DisplayName = "Roster", Icon = "fas fa-calendar-alt", Route = "Roster", DisplayOrder = 10, IsActive = true }
        };
        var subFinance = new List<SubMenu>
        {
            new SubMenu { TenantId = tenant.Id, MenuId = menuFinance.Id, Name = "Bills", DisplayName = "Bills", Icon = "fas fa-file-invoice", Route = "Bills", DisplayOrder = 1, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuFinance.Id, Name = "Wages", DisplayName = "Wages", Icon = "fas fa-money-bill-wave", Route = "Wages", DisplayOrder = 2, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuFinance.Id, Name = "Clients", DisplayName = "Clients", Icon = "fas fa-building", Route = "Clients", DisplayOrder = 3, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuFinance.Id, Name = "Contracts", DisplayName = "Contracts", Icon = "fas fa-file-contract", Route = "Contracts", DisplayOrder = 4, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuFinance.Id, Name = "Payments", DisplayName = "Payments", Icon = "fas fa-money-check", Route = "Payments", DisplayOrder = 5, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuFinance.Id, Name = "Expenses", DisplayName = "Expenses", Icon = "fas fa-receipt", Route = "Expenses", DisplayOrder = 6, IsActive = true }
        };
        var subHr = new List<SubMenu>
        {
            new SubMenu { TenantId = tenant.Id, MenuId = menuHr.Id, Name = "LeaveRequests", DisplayName = "Leave Requests", Icon = "fas fa-calendar-times", Route = "LeaveRequests", DisplayOrder = 1, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuHr.Id, Name = "TrainingRecords", DisplayName = "Training", Icon = "fas fa-graduation-cap", Route = "TrainingRecords", DisplayOrder = 2, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuHr.Id, Name = "Equipment", DisplayName = "Equipment", Icon = "fas fa-tools", Route = "Equipment", DisplayOrder = 3, IsActive = true }
        };
        var subMore = new List<SubMenu>
        {
            new SubMenu { TenantId = tenant.Id, MenuId = menuMore.Id, Name = "CompanyProfile", DisplayName = "Company Profile", Icon = "fas fa-building", Route = "CompanyProfile", DisplayOrder = 0, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuMore.Id, Name = "Compliance", DisplayName = "Compliance", Icon = "fas fa-clipboard-check", Route = "Compliance", DisplayOrder = 1, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuMore.Id, Name = "Announcements", DisplayName = "Announcements", Icon = "fas fa-bullhorn", Route = "Announcements", DisplayOrder = 2, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuMore.Id, Name = "Notifications", DisplayName = "Notifications", Icon = "fas fa-bell", Route = "Notifications", DisplayOrder = 3, IsActive = true },
            new SubMenu { TenantId = tenant.Id, MenuId = menuMore.Id, Name = "Reports", DisplayName = "Monthly Report", Icon = "fas fa-file-excel", Route = "Reports", DisplayOrder = 4, IsActive = true }
        };
        context.SubMenus.AddRange(subAdmin);
        context.SubMenus.AddRange(subOps);
        context.SubMenus.AddRange(subFinance);
        context.SubMenus.AddRange(subHr);
        context.SubMenus.AddRange(subMore);
        await context.SaveChangesAsync();

        var allSubMenus = subAdmin.Concat(subOps).Concat(subFinance).Concat(subHr).Concat(subMore).ToList();

        // Administrator: all menus + all submenus
        foreach (var menu in mainMenus)
            context.RoleMenus.Add(new RoleMenu { RoleId = adminRole.Id, MenuId = menu.Id });
        foreach (var sub in allSubMenus)
            context.RoleSubMenus.Add(new RoleSubMenu { RoleId = adminRole.Id, SubMenuId = sub.Id });
        await context.SaveChangesAsync();

        // Accounts: Dashboard + Finance menu and all finance submenus
        context.RoleMenus.Add(new RoleMenu { RoleId = accountsRole.Id, MenuId = menuDashboard.Id });
        context.RoleMenus.Add(new RoleMenu { RoleId = accountsRole.Id, MenuId = menuFinance.Id });
        foreach (var sub in subFinance)
            context.RoleSubMenus.Add(new RoleSubMenu { RoleId = accountsRole.Id, SubMenuId = sub.Id });
        await context.SaveChangesAsync();

        // Security Guard: Dashboard + Operations (incl. Roster) + More (Company Profile only)
        context.RoleMenus.Add(new RoleMenu { RoleId = guardRole.Id, MenuId = menuDashboard.Id });
        context.RoleMenus.Add(new RoleMenu { RoleId = guardRole.Id, MenuId = menuOps.Id });
        context.RoleMenus.Add(new RoleMenu { RoleId = guardRole.Id, MenuId = menuMore.Id });
        foreach (var sub in subOps)
            context.RoleSubMenus.Add(new RoleSubMenu { RoleId = guardRole.Id, SubMenuId = sub.Id });
        context.RoleSubMenus.Add(new RoleSubMenu { RoleId = guardRole.Id, SubMenuId = subMore[0].Id }); // Company Profile
        await context.SaveChangesAsync();

        // Supervisor: Dashboard + Operations (incl. Roster) + HR + More (Company Profile)
        context.RoleMenus.Add(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuDashboard.Id });
        context.RoleMenus.Add(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuOps.Id });
        context.RoleMenus.Add(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuHr.Id });
        context.RoleMenus.Add(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuMore.Id });
        foreach (var sub in subOps.Concat(subHr))
            context.RoleSubMenus.Add(new RoleSubMenu { RoleId = supervisorRole.Id, SubMenuId = sub.Id });
        context.RoleSubMenus.Add(new RoleSubMenu { RoleId = supervisorRole.Id, SubMenuId = subMore[0].Id }); // Company Profile
        await context.SaveChangesAsync();

        return true; // Seed ran and data was inserted
    }

    /// <summary>
    /// For existing databases: ensure new menu structure exists. If tenant has no "Administration" main menu, they use old flat structure; no auto-migration here.
    /// Full menu hierarchy (Main + SubMenus) is created only in SeedAsync on fresh DB. Reset DB + run app to get new structure.
    /// </summary>
    public static Task EnsureNewMenusAsync(ApplicationDbContext context)
    {
        return Task.CompletedTask;
    }
}
