using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public DeleteRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
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
            return ApiResponse<bool>.ErrorResponse("Cannot delete system role");
        }

        // Check if role has users
        var userCount = await _unitOfWork.Repository<UserRole>().CountAsync(
            ur => ur.RoleId == role.Id,
            cancellationToken);

        if (userCount > 0)
        {
            return ApiResponse<bool>.ErrorResponse("Cannot delete role with assigned users. Please unassign users first.");
        }

        // Soft delete - mark as inactive
        role.IsActive = false;
        role.ModifiedDate = DateTime.UtcNow;
        await roleRepo.UpdateAsync(role, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Role deleted successfully");
    }
}
