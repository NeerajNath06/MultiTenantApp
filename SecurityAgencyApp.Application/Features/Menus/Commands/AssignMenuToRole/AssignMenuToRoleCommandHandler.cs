using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.AssignMenuToRole;

public class AssignMenuToRoleCommandHandler : IRequestHandler<AssignMenuToRoleCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public AssignMenuToRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(AssignMenuToRoleCommand request, CancellationToken cancellationToken)
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

        foreach (var existingMenu in existingMenus)
        {
            await _unitOfWork.Repository<RoleMenu>().DeleteAsync(existingMenu, cancellationToken);
        }

        // Add new menu assignments
        foreach (var menuId in request.MenuIds)
        {
            // Verify menu exists and belongs to tenant
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Menus assigned to role successfully");
    }
}
