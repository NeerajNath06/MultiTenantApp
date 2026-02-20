using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Authentication.Commands.RegisterAgency;

public class RegisterAgencyCommandHandler : IRequestHandler<RegisterAgencyCommand, ApiResponse<RegisterAgencyResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterAgencyCommandHandler(IUnitOfWork unitOfWork, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<RegisterAgencyResponseDto>> Handle(RegisterAgencyCommand request, CancellationToken cancellationToken)
    {
        // Check if registration number already exists (tenant table has no tenant filter)
        var tenantRepo = _unitOfWork.Repository<Tenant>();
        var existingTenant = await tenantRepo.FirstOrDefaultAsync(
            t => t.RegistrationNumber == request.RegistrationNumber || t.Email == request.Email,
            cancellationToken);

        if (existingTenant != null)
        {
            return ApiResponse<RegisterAgencyResponseDto>.ErrorResponse(
                "Agency with this registration number or email already exists");
        }

        // Check if admin username or email already exists (no tenant context yet – use IgnoreFilters)
        var userRepo = _unitOfWork.Repository<User>();
        var existingUser = await userRepo.FirstOrDefaultIgnoreFiltersAsync(
            u => u.UserName == request.AdminUserName || u.Email == request.AdminEmail,
            cancellationToken);

        if (existingUser != null)
        {
            return ApiResponse<RegisterAgencyResponseDto>.ErrorResponse(
                "Admin username or email already exists");
        }

        // Create Tenant
        var tenant = new Tenant
        {
            CompanyName = request.CompanyName,
            RegistrationNumber = request.RegistrationNumber,
            Email = request.Email,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country ?? "India",
            PinCode = request.PinCode,
            IsActive = true,
            SubscriptionStartDate = DateTime.UtcNow,
            SubscriptionEndDate = DateTime.UtcNow.AddYears(1) // Default 1 year subscription
        };

        await tenantRepo.AddAsync(tenant, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create default permissions (if not exists globally)
        var permissionRepo = _unitOfWork.Repository<Permission>();
        var existingPermissions = await permissionRepo.GetAllAsync(cancellationToken);
        
        if (!existingPermissions.Any())
        {
            var defaultPermissions = new List<Permission>
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
                new Permission { Resource = "Menus", Action = "Delete", Description = "Delete menus" },
                new Permission { Resource = "FormBuilder", Action = "Create", Description = "Create forms" },
                new Permission { Resource = "FormBuilder", Action = "Read", Description = "View forms" },
                new Permission { Resource = "FormBuilder", Action = "Update", Description = "Update forms" },
                new Permission { Resource = "FormBuilder", Action = "Delete", Description = "Delete forms" }
            };
            foreach (var permission in defaultPermissions)
            {
                await permissionRepo.AddAsync(permission, cancellationToken);
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            existingPermissions = defaultPermissions;
        }

        // Create Administrator Role for this tenant
        var adminRole = new Role
        {
            TenantId = tenant.Id,
            Name = "Administrator",
            Code = "ADMIN",
            Description = "Full system access",
            IsSystemRole = true,
            IsActive = true
        };
        await _unitOfWork.Repository<Role>().AddAsync(adminRole, cancellationToken);
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
        await _unitOfWork.Repository<Role>().AddAsync(guardRole, cancellationToken);
        await _unitOfWork.Repository<Role>().AddAsync(supervisorRole, cancellationToken);
        await _unitOfWork.Repository<Role>().AddAsync(accountsRole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign all permissions to admin role
        foreach (var permission in existingPermissions)
        {
            await _unitOfWork.Repository<RolePermission>().AddAsync(new RolePermission
            {
                RoleId = adminRole.Id,
                PermissionId = permission.Id
            }, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create default department
        var department = new Department
        {
            TenantId = tenant.Id,
            Name = "Administration",
            Code = "ADMIN",
            Description = "Administration Department",
            IsActive = true
        };
        await _unitOfWork.Repository<Department>().AddAsync(department, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Create Admin User
        var passwordHash = _passwordHasher.HashPassword(request.AdminPassword);
        var adminUser = new User
        {
            TenantId = tenant.Id,
            UserName = request.AdminUserName,
            Email = request.AdminEmail,
            PasswordHash = passwordHash,
            Password = request.AdminPassword,
            FirstName = request.AdminFirstName,
            LastName = request.AdminLastName,
            PhoneNumber = request.AdminPhoneNumber,
            DepartmentId = department.Id,
            IsActive = true,
            IsEmailVerified = false
        };
        await userRepo.AddAsync(adminUser, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign admin role to admin user
        await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole
        {
            UserId = adminUser.Id,
            RoleId = adminRole.Id
        }, cancellationToken);

        // Create full Main + SubMenu hierarchy (same as seed – enterprise: new agency gets complete menus)
        var menuDashboard = new Menu { TenantId = tenant.Id, Name = "Dashboard", DisplayName = "Dashboard", Icon = "fas fa-home", Route = "Home", DisplayOrder = 1, IsActive = true };
        var menuAdmin = new Menu { TenantId = tenant.Id, Name = "Administration", DisplayName = "Administration", Icon = "fas fa-cog", Route = "#", DisplayOrder = 2, IsActive = true };
        var menuOps = new Menu { TenantId = tenant.Id, Name = "Operations", DisplayName = "Operations", Icon = "fas fa-tasks", Route = "#", DisplayOrder = 3, IsActive = true };
        var menuFinance = new Menu { TenantId = tenant.Id, Name = "Finance", DisplayName = "Finance", Icon = "fas fa-money-bill-wave", Route = "#", DisplayOrder = 4, IsActive = true };
        var menuHr = new Menu { TenantId = tenant.Id, Name = "HR", DisplayName = "HR", Icon = "fas fa-users-cog", Route = "#", DisplayOrder = 5, IsActive = true };
        var menuMore = new Menu { TenantId = tenant.Id, Name = "More", DisplayName = "More", Icon = "fas fa-ellipsis-h", Route = "#", DisplayOrder = 6, IsActive = true };
        foreach (var m in new[] { menuDashboard, menuAdmin, menuOps, menuFinance, menuHr, menuMore })
            await _unitOfWork.Repository<Menu>().AddAsync(m, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
        foreach (var s in subAdmin.Concat(subOps).Concat(subFinance).Concat(subHr).Concat(subMore))
            await _unitOfWork.Repository<SubMenu>().AddAsync(s, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var mainMenus = new[] { menuDashboard, menuAdmin, menuOps, menuFinance, menuHr, menuMore };
        var allSubMenus = subAdmin.Concat(subOps).Concat(subFinance).Concat(subHr).Concat(subMore).ToList();

        foreach (var menu in mainMenus)
            await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = adminRole.Id, MenuId = menu.Id }, cancellationToken);
        foreach (var sub in allSubMenus)
            await _unitOfWork.Repository<RoleSubMenu>().AddAsync(new RoleSubMenu { RoleId = adminRole.Id, SubMenuId = sub.Id }, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = accountsRole.Id, MenuId = menuDashboard.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = accountsRole.Id, MenuId = menuFinance.Id }, cancellationToken);
        foreach (var sub in subFinance)
            await _unitOfWork.Repository<RoleSubMenu>().AddAsync(new RoleSubMenu { RoleId = accountsRole.Id, SubMenuId = sub.Id }, cancellationToken);

        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = guardRole.Id, MenuId = menuDashboard.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = guardRole.Id, MenuId = menuOps.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = guardRole.Id, MenuId = menuMore.Id }, cancellationToken);
        foreach (var sub in subOps)
            await _unitOfWork.Repository<RoleSubMenu>().AddAsync(new RoleSubMenu { RoleId = guardRole.Id, SubMenuId = sub.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleSubMenu>().AddAsync(new RoleSubMenu { RoleId = guardRole.Id, SubMenuId = subMore[0].Id }, cancellationToken);

        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuDashboard.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuOps.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuHr.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu { RoleId = supervisorRole.Id, MenuId = menuMore.Id }, cancellationToken);
        foreach (var sub in subOps.Concat(subHr))
            await _unitOfWork.Repository<RoleSubMenu>().AddAsync(new RoleSubMenu { RoleId = supervisorRole.Id, SubMenuId = sub.Id }, cancellationToken);
        await _unitOfWork.Repository<RoleSubMenu>().AddAsync(new RoleSubMenu { RoleId = supervisorRole.Id, SubMenuId = subMore[0].Id }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new RegisterAgencyResponseDto
        {
            TenantId = tenant.Id,
            CompanyName = tenant.CompanyName,
            AdminUserId = adminUser.Id,
            AdminUserName = adminUser.UserName,
            Message = "Agency registered successfully. You can now login with your credentials."
        };

        return ApiResponse<RegisterAgencyResponseDto>.SuccessResponse(response, "Agency registered successfully");
    }
}
