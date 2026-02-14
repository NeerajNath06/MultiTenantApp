using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Commands.UpdateMenu;

public class UpdateMenuCommandHandler : IRequestHandler<UpdateMenuCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateMenuCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateMenuCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var menuRepo = _unitOfWork.Repository<Menu>();
        var menu = await menuRepo.GetByIdAsync(request.Id, cancellationToken);

        if (menu == null || menu.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Menu not found");
        }

        menu.Name = request.Name;
        menu.DisplayName = request.DisplayName ?? request.Name;
        menu.Icon = request.Icon;
        menu.Route = request.Route;
        menu.DisplayOrder = request.DisplayOrder;
        menu.IsActive = request.IsActive;
        menu.ModifiedDate = DateTime.UtcNow;

        await menuRepo.UpdateAsync(menu, cancellationToken);

        // Update permissions if provided
        if (request.PermissionIds != null)
        {
            // Remove existing permissions
            var existingPermissions = await _unitOfWork.Repository<MenuPermission>().FindAsync(
                mp => mp.MenuId == menu.Id, cancellationToken);
            
            foreach (var perm in existingPermissions)
            {
                await _unitOfWork.Repository<MenuPermission>().DeleteAsync(perm, cancellationToken);
            }

            // Add new permissions
            foreach (var permissionId in request.PermissionIds)
            {
                var menuPermission = new MenuPermission
                {
                    MenuId = menu.Id,
                    PermissionId = permissionId
                };
                await _unitOfWork.Repository<MenuPermission>().AddAsync(menuPermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true, "Menu updated successfully");
    }
}
