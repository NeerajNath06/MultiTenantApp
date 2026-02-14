using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.UpdateRole;

public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var roleRepo = _unitOfWork.Repository<Role>();
        var role = await roleRepo.GetByIdAsync(request.Id, cancellationToken);

        if (role == null || role.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Role not found");
        }

        if (role.IsSystemRole)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot modify system role");
        }

        // Check if code is already taken by another role
        if (role.Code != request.Code)
        {
            var existing = await roleRepo.FirstOrDefaultAsync(
                r => r.Code == request.Code && r.Id != request.Id && r.TenantId == _tenantContext.TenantId.Value,
                cancellationToken);

            if (existing != null)
            {
                return ApiResponse<bool>.ErrorResponse("Role code already exists");
            }
        }

        role.Name = request.Name;
        role.Code = request.Code;
        role.Description = request.Description;
        role.IsActive = request.IsActive;
        role.ModifiedDate = DateTime.UtcNow;

        await roleRepo.UpdateAsync(role, cancellationToken);

        // Update permissions if provided
        if (request.PermissionIds != null)
        {
            // Remove existing permissions
            var existingPermissions = await _unitOfWork.Repository<RolePermission>().FindAsync(
                rp => rp.RoleId == role.Id, cancellationToken);

            foreach (var perm in existingPermissions)
            {
                await _unitOfWork.Repository<RolePermission>().DeleteAsync(perm, cancellationToken);
            }

            // Add new permissions
            foreach (var permissionId in request.PermissionIds)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                };
                await _unitOfWork.Repository<RolePermission>().AddAsync(rolePermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Role updated successfully");
    }
}
