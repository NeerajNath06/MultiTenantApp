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
        // Check if registration number already exists
        var tenantRepo = _unitOfWork.Repository<Tenant>();
        var existingTenant = await tenantRepo.FirstOrDefaultAsync(
            t => t.RegistrationNumber == request.RegistrationNumber || t.Email == request.Email,
            cancellationToken);

        if (existingTenant != null)
        {
            return ApiResponse<RegisterAgencyResponseDto>.ErrorResponse(
                "Agency with this registration number or email already exists");
        }

        // Check if admin username or email already exists
        var userRepo = _unitOfWork.Repository<User>();
        var existingUser = await userRepo.FirstOrDefaultAsync(
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
        await _unitOfWork.Repository<Role>().AddAsync(guardRole, cancellationToken);
        await _unitOfWork.Repository<Role>().AddAsync(supervisorRole, cancellationToken);
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

        // Create default menus for the tenant
        var menus = new List<Menu>
        {
            new Menu
            {
                TenantId = tenant.Id,
                Name = "Dashboard",
                DisplayName = "Dashboard",
                Icon = "fas fa-home",
                Route = "/Home",
                DisplayOrder = 1,
                IsActive = true
            },
            new Menu
            {
                TenantId = tenant.Id,
                Name = "Users",
                DisplayName = "User Management",
                Icon = "fas fa-users",
                Route = "/Users",
                DisplayOrder = 2,
                IsActive = true
            },
            new Menu
            {
                TenantId = tenant.Id,
                Name = "Departments",
                DisplayName = "Departments",
                Icon = "fas fa-building",
                Route = "/Departments",
                DisplayOrder = 3,
                IsActive = true
            },
            new Menu
            {
                TenantId = tenant.Id,
                Name = "Roles",
                DisplayName = "Roles",
                Icon = "fas fa-user-tag",
                Route = "/Roles",
                DisplayOrder = 4,
                IsActive = true
            },
            new Menu
            {
                TenantId = tenant.Id,
                Name = "Menus",
                DisplayName = "Menu Management",
                Icon = "fas fa-list",
                Route = "/Menus",
                DisplayOrder = 5,
                IsActive = true
            },
            new Menu
            {
                TenantId = tenant.Id,
                Name = "FormBuilder",
                DisplayName = "Form Builder",
                Icon = "fas fa-file-alt",
                Route = "/FormBuilder",
                DisplayOrder = 6,
                IsActive = true
            }
        };
        foreach (var menu in menus)
        {
            await _unitOfWork.Repository<Menu>().AddAsync(menu, cancellationToken);
        }
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Assign menus to admin role
        foreach (var menu in menus)
        {
            await _unitOfWork.Repository<RoleMenu>().AddAsync(new RoleMenu
            {
                RoleId = adminRole.Id,
                MenuId = menu.Id
            }, cancellationToken);
        }
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
