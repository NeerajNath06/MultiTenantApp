using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.CreateGuard;

public class CreateGuardCommandHandler : IRequestHandler<CreateGuardCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;

    public CreateGuardCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateGuardCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Generate guard code
        var guardRepo = _unitOfWork.Repository<SecurityGuard>();
        var existingGuards = await guardRepo.FindAsync(
            g => g.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        var guardCount = existingGuards.Count();
        var guardCode = $"GRD{(guardCount + 1):D4}";

        // Check if code already exists (unlikely but safe)
        while (existingGuards.Any(g => g.GuardCode == guardCode))
        {
            guardCount++;
            guardCode = $"GRD{guardCount:D4}";
        }

        var guard = new SecurityGuard
        {
            TenantId = _tenantContext.TenantId.Value,
            GuardCode = guardCode,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            AlternatePhone = request.AlternatePhone,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PinCode = request.PinCode,
            AadharNumber = request.AadharNumber,
            PANNumber = request.PANNumber,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            JoiningDate = request.JoiningDate,
            IsActive = true,
            SupervisorId = request.SupervisorId
        };

        await guardRepo.AddAsync(guard, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // Optionally create login account for mobile app
        if (request.CreateLoginAccount && !string.IsNullOrWhiteSpace(request.LoginPassword) && request.LoginPassword.Length >= 6)
        {
            var loginUserName = (request.LoginUserName ?? request.Email ?? guard.GuardCode).Trim();
            if (string.IsNullOrEmpty(loginUserName))
            {
                return ApiResponse<Guid>.ErrorResponse("Login username or email is required when creating login account");
            }
            var userRepo = _unitOfWork.Repository<User>();
            var existingUser = await userRepo.FirstOrDefaultAsync(
                u => (u.UserName == loginUserName || u.Email == (request.Email ?? "")) && u.TenantId == _tenantContext.TenantId.Value,
                cancellationToken);
            if (existingUser != null)
            {
                return ApiResponse<Guid>.ErrorResponse("A user with this username or email already exists");
            }
            var roleRepo = _unitOfWork.Repository<Role>();
            var guardRole = await roleRepo.FirstOrDefaultAsync(
                r => r.TenantId == _tenantContext.TenantId.Value && r.Code == "GUARD",
                cancellationToken);
            if (guardRole == null)
            {
                // Ensure GUARD role exists for this tenant (e.g. old DB seed may not have it)
                guardRole = new Role
                {
                    TenantId = _tenantContext.TenantId.Value,
                    Name = "Security Guard",
                    Code = "GUARD",
                    Description = "Security guard – can use mobile app",
                    IsSystemRole = true,
                    IsActive = true
                };
                await roleRepo.AddAsync(guardRole, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            var passwordHash = _passwordHasher.HashPassword(request.LoginPassword);
            var appUser = new User
            {
                TenantId = _tenantContext.TenantId.Value,
                UserName = loginUserName,
                Email = request.Email ?? $"{guard.GuardCode}@guard.local",
                PasswordHash = passwordHash,
                Password = request.LoginPassword,
                FirstName = guard.FirstName,
                LastName = guard.LastName,
                PhoneNumber = guard.PhoneNumber,
                IsActive = true
            };
            await userRepo.AddAsync(appUser, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            guard.UserId = appUser.Id;
            await guardRepo.UpdateAsync(guard, cancellationToken);
            await _unitOfWork.Repository<UserRole>().AddAsync(new UserRole { UserId = appUser.Id, RoleId = guardRole.Id }, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<Guid>.SuccessResponse(guard.Id, "Security guard created successfully");
    }
}
