using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.AssignMenusToRole;

public class AssignMenusToRoleCommandHandler : IRequestHandler<AssignMenusToRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignMenusToRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(AssignMenusToRoleCommand request, CancellationToken cancellationToken)
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

        // Remove existing menu assignments
        var existingMenus = await _unitOfWork.Repository<RoleMenu>().FindAsync(
            rm => rm.RoleId == request.RoleId, cancellationToken);
        foreach (var menu in existingMenus)
        {
            await _unitOfWork.Repository<RoleMenu>().DeleteAsync(menu, cancellationToken);
        }

        // Remove existing submenu assignments
        var existingSubMenus = await _unitOfWork.Repository<RoleSubMenu>().FindAsync(
            rsm => rsm.RoleId == request.RoleId, cancellationToken);
        foreach (var subMenu in existingSubMenus)
        {
            await _unitOfWork.Repository<RoleSubMenu>().DeleteAsync(subMenu, cancellationToken);
        }

        // Add new menu assignments
        foreach (var menuId in request.MenuIds)
        {
            var menu = await _unitOfWork.Repository<Menu>().GetByIdAsync(menuId, cancellationToken);
            if (menu != null && menu.TenantId == _tenantContext.TenantId.Value)
            {
                var roleMenu = new RoleMenu
                {
                    RoleId = request.RoleId,
                    MenuId = menuId
                };
                await _unitOfWork.Repository<RoleMenu>().AddAsync(roleMenu, cancellationToken);
            }
        }

        // Add new submenu assignments
        foreach (var subMenuId in request.SubMenuIds)
        {
            var subMenu = await _unitOfWork.Repository<SubMenu>().GetByIdAsync(subMenuId, cancellationToken);
            if (subMenu != null && subMenu.Menu.TenantId == _tenantContext.TenantId.Value)
            {
                var roleSubMenu = new RoleSubMenu
                {
                    RoleId = request.RoleId,
                    SubMenuId = subMenuId
                };
                await _unitOfWork.Repository<RoleSubMenu>().AddAsync(roleSubMenu, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Menus and SubMenus assigned to role successfully");
    }
}
