using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleById;

public class GetRoleByIdQueryHandler : IRequestHandler<GetRoleByIdQuery, ApiResponse<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetRoleByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<RoleDto>> Handle(GetRoleByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<RoleDto>.ErrorResponse("Tenant context not found");
        }

        var role = await _unitOfWork.Repository<Role>().GetByIdAsync(request.Id, cancellationToken);
        
        if (role == null || role.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<RoleDto>.ErrorResponse("Role not found");
        }

        // Get permissions
        var rolePermissions = await _unitOfWork.Repository<RolePermission>().FindAsync(
            rp => rp.RoleId == role.Id, cancellationToken);
        var permissionIds = rolePermissions.Select(rp => rp.PermissionId).ToList();

        // Get menus
        var roleMenus = await _unitOfWork.Repository<RoleMenu>().FindAsync(
            rm => rm.RoleId == role.Id, cancellationToken);
        var menuIds = roleMenus.Select(rm => rm.MenuId).ToList();

        // Get submenus
        var roleSubMenus = await _unitOfWork.Repository<RoleSubMenu>().FindAsync(
            rsm => rsm.RoleId == role.Id, cancellationToken);
        var subMenuIds = roleSubMenus.Select(rsm => rsm.SubMenuId).ToList();

        var roleDto = new RoleDto
        {
            Id = role.Id,
            Name = role.Name,
            Code = role.Code,
            Description = role.Description,
            IsSystemRole = role.IsSystemRole,
            IsActive = role.IsActive,
            PermissionIds = permissionIds,
            MenuIds = menuIds,
            SubMenuIds = subMenuIds
        };

        return ApiResponse<RoleDto>.SuccessResponse(roleDto, "Role retrieved successfully");
    }
}
