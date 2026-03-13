using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Users.Commands.CreateUser;

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;

    public CreateUserCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Check if username or email already exists
        var userRepo = _unitOfWork.Repository<User>();
        var existingUser = await userRepo.FirstOrDefaultAsync(
            u => (u.UserName == request.UserName || u.Email == request.Email) && 
                 u.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existingUser != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Username or email already exists");
        }

        // Hash password
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        var user = new User
        {
            TenantId = _tenantContext.TenantId.Value,
            UserName = request.UserName.Trim(),
            Email = request.Email.Trim(),
            PasswordHash = passwordHash,
            Password = request.Password,
            FirstName = request.FirstName.Trim(),
            LastName = string.IsNullOrWhiteSpace(request.LastName) ? string.Empty : request.LastName.Trim(),
            PhoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim(),
            AadharNumber = string.IsNullOrWhiteSpace(request.AadharNumber) ? null : request.AadharNumber.Trim(),
            PANNumber = string.IsNullOrWhiteSpace(request.PANNumber) ? null : request.PANNumber.Trim().ToUpperInvariant(),
            UAN = string.IsNullOrWhiteSpace(request.UAN) ? null : request.UAN.Trim(),
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            IsActive = true,
            IsEmailVerified = false
        };

        await userRepo.AddAsync(user, cancellationToken);

        // Assign roles if provided
        if (request.RoleIds != null && request.RoleIds.Any())
        {
            foreach (var roleId in request.RoleIds)
            {
                var userRole = new UserRole
                {
                    UserId = user.Id,
                    RoleId = roleId
                };
                await _unitOfWork.Repository<UserRole>().AddAsync(userRole, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(user.Id, "User created successfully");
    }
}
