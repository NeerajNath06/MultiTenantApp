using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly IPasswordHasher _passwordHasher;

    public UpdateUserCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, IPasswordHasher passwordHasher)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _passwordHasher = passwordHasher;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.Id, cancellationToken);

        if (user == null || user.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("User not found");
        }

        // Check if email is already taken by another user
        if (user.Email != request.Email)
        {
            var existingUser = await userRepo.FirstOrDefaultAsync(
                u => u.Email == request.Email && u.Id != request.Id && u.TenantId == _tenantContext.TenantId.Value,
                cancellationToken);

            if (existingUser != null)
            {
                return ApiResponse<bool>.ErrorResponse("Email is already taken");
            }
        }

        // Update user properties
        user.FirstName = request.FirstName;
        user.LastName = request.LastName ?? string.Empty;
        user.Email = request.Email;
        user.PhoneNumber = request.PhoneNumber;
        user.AadharNumber = request.AadharNumber;
        user.PANNumber = request.PANNumber;
        user.UAN = request.UAN;
        user.DepartmentId = request.DepartmentId;
        user.DesignationId = request.DesignationId;
        user.IsActive = request.IsActive;
        user.ModifiedDate = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            user.PasswordHash = _passwordHasher.HashPassword(request.NewPassword);
            user.Password = request.NewPassword;
        }

        await userRepo.UpdateAsync(user, cancellationToken);

        // Update roles if provided
        if (request.RoleIds != null)
        {
            // Remove existing roles
            var existingRoles = await _unitOfWork.Repository<UserRole>().FindAsync(
                ur => ur.UserId == user.Id, cancellationToken);

            foreach (var existingRole in existingRoles)
            {
                await _unitOfWork.Repository<UserRole>().DeleteAsync(existingRole, cancellationToken);
            }

            // Add new roles
            foreach (var roleId in request.RoleIds)
            {
                var role = await _unitOfWork.Repository<Role>().GetByIdAsync(roleId, cancellationToken);
                if (role != null && role.TenantId == _tenantContext.TenantId.Value)
                {
                    var userRole = new UserRole
                    {
                        UserId = user.Id,
                        RoleId = roleId
                    };
                    await _unitOfWork.Repository<UserRole>().AddAsync(userRole, cancellationToken);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "User updated successfully");
    }
}
