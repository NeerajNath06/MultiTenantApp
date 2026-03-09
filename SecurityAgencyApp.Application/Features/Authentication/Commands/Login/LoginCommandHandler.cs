using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Authentication.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponse<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IJwtService _jwtService;
    private readonly IPasswordHasher _passwordHasher;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService,
        IJwtService jwtService,
        IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
        _jwtService = jwtService;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var loginId = !string.IsNullOrWhiteSpace(request.UserName) ? request.UserName.Trim() : (request.UserEmail ?? "").Trim();
        if (string.IsNullOrWhiteSpace(loginId))
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid username or password");
        }
        var loginIdLower = loginId.ToLowerInvariant();
        // Login: user lookup must ignore global tenant filter (no tenant context yet). Case-insensitive so Guard/any user can login with email.
        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.FirstOrDefaultIgnoreFiltersAsync(
            u => (u.UserName != null && u.UserName.ToLower() == loginIdLower) || (u.Email != null && u.Email.ToLower() == loginIdLower),
            cancellationToken);

        if (user == null || !user.IsActive)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid username or password");
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Account has no password set. Use Reset password to set one.");
        }

        // Verify password
        if (!_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Invalid username or password");
        }

        // Get roles early to block Guard-only users from web portal (guards use mobile app only)
        var userRolesForCheck = await _unitOfWork.Repository<UserRole>().FindAsync(
            ur => ur.UserId == user.Id, cancellationToken);
        var roleIdsForCheck = userRolesForCheck.Select(ur => ur.RoleId).ToList();
        var rolesForCheck = await _unitOfWork.Repository<Role>().FindIgnoreFiltersAsync(
            r => roleIdsForCheck.Contains(r.Id), cancellationToken);
        var roleCodes = rolesForCheck.Select(r => r.Code).ToList();
        bool isGuardOnly = roleCodes.Any(c => string.Equals(c, "GUARD", StringComparison.OrdinalIgnoreCase))
            && !roleCodes.Any(c => string.Equals(c, "ADMIN", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c, "SUPERVISOR", StringComparison.OrdinalIgnoreCase)
                || string.Equals(c, "ACCOUNTS", StringComparison.OrdinalIgnoreCase));
        if (isGuardOnly)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse(
                "Security guards cannot log in to the web portal. Please use the Security Agency mobile app to sign in.");
        }

        // Update last login
        user.LastLoginDate = DateTime.UtcNow;
        await _unitOfWork.Repository<User>().UpdateAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Get tenant
        var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(user.TenantId, cancellationToken);
        
        // Get roles and permissions
        var userRoles = await _unitOfWork.Repository<UserRole>().FindAsync(
            ur => ur.UserId == user.Id, cancellationToken);
        
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = await _unitOfWork.Repository<Role>().FindIgnoreFiltersAsync(
            r => roleIds.Contains(r.Id), cancellationToken);

        // Get role permissions
        var rolePermissionIds = roles.SelectMany(r => r.RolePermissions.Select(rp => rp.PermissionId)).Distinct().ToList();
        var permissions = rolePermissionIds.Any()
            ? await _unitOfWork.Repository<Permission>().FindAsync(p => rolePermissionIds.Contains(p.Id), cancellationToken)
            : new List<Permission>();

        // Get user direct permissions
        var userPermissions = await _unitOfWork.Repository<UserPermission>().FindAsync(
            up => up.UserId == user.Id && up.IsGranted, cancellationToken);
        var userPermissionIds = userPermissions.Select(up => up.PermissionId).ToList();
        var directPermissions = userPermissionIds.Any()
            ? await _unitOfWork.Repository<Permission>().FindAsync(p => userPermissionIds.Contains(p.Id), cancellationToken)
            : new List<Permission>();

        // Combine all permissions
        var allPermissions = permissions.Union(directPermissions).Select(p => $"{p.Resource}.{p.Action}").Distinct().ToList();

        // Generate tokens
        if (tenant == null)
        {
            return ApiResponse<LoginResponseDto>.ErrorResponse("Tenant not found");
        }
        var accessToken = _jwtService.GenerateToken(user, tenant, roles.Select(r => r.Name).ToList(), allPermissions);
        var refreshToken = _jwtService.GenerateRefreshToken();

        var roleNames = roles.Select(r => r.Name).ToList();
        Guid? guardId = null;
        // Treat "Supervisor" or "Security Supervisor" (or any role name containing "Supervisor") as supervisor
        bool isSupervisor = roleNames.Any(r => (r ?? "").IndexOf("Supervisor", StringComparison.OrdinalIgnoreCase) >= 0);
        if (roleNames.Any(r => string.Equals(r, "Security Guard", StringComparison.OrdinalIgnoreCase) || isSupervisor))
        {
            var guardRepo = _unitOfWork.Repository<SecurityGuard>();
            var linkedGuard = await guardRepo.FirstOrDefaultIgnoreFiltersAsync(
                g => g.UserId == user.Id,
                cancellationToken);
            if (linkedGuard != null)
                guardId = linkedGuard.Id;
        }

        var response = new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = 900, // 15 minutes
            User = new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                TenantId = user.TenantId,
                TenantName = tenant?.CompanyName ?? string.Empty,
                Roles = roleNames,
                Permissions = allPermissions,
                GuardId = guardId,
                IsSupervisor = isSupervisor
            }
        };

        return ApiResponse<LoginResponseDto>.SuccessResponse(response, "Login successful");
    }
}
