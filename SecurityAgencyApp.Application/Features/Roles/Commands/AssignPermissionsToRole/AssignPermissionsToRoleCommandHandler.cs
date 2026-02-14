using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.AssignPermissionsToRole;

public class AssignPermissionsToRoleCommandHandler : IRequestHandler<AssignPermissionsToRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignPermissionsToRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(AssignPermissionsToRoleCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        // Verify role exists and belongs to tenant
        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null || role.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Role not found");
        }

        // Remove existing permissions
        var existingPermissions = await _unitOfWork.Repository<RolePermission>().FindAsync(
            rp => rp.RoleId == request.RoleId, cancellationToken);

        foreach (var perm in existingPermissions)
        {
            await _unitOfWork.Repository<RolePermission>().DeleteAsync(perm, cancellationToken);
        }

        // Add new permissions
        foreach (var permissionId in request.PermissionIds)
        {
            var rolePermission = new RolePermission
            {
                RoleId = request.RoleId,
                PermissionId = permissionId
            };
            await _unitOfWork.Repository<RolePermission>().AddAsync(rolePermission, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Permissions assigned to role successfully");
    }
}
