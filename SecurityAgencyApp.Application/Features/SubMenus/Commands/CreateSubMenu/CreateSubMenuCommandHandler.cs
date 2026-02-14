using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SubMenus.Commands.CreateSubMenu;

public class CreateSubMenuCommandHandler : IRequestHandler<CreateSubMenuCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateSubMenuCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateSubMenuCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Verify menu exists and belongs to tenant
        var menu = await _unitOfWork.Repository<Menu>().GetByIdAsync(request.MenuId, cancellationToken);
        if (menu == null || menu.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<Guid>.ErrorResponse("Menu not found");
        }

        var subMenu = new SubMenu
        {
            TenantId = _tenantContext.TenantId.Value,
            MenuId = request.MenuId,
            Name = request.Name,
            DisplayName = request.DisplayName ?? request.Name,
            Icon = request.Icon,
            Route = request.Route,
            DisplayOrder = request.DisplayOrder,
            IsActive = request.IsActive
        };

        await _unitOfWork.Repository<SubMenu>().AddAsync(subMenu, cancellationToken);

        // Add permissions if provided
        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            foreach (var permissionId in request.PermissionIds)
            {
                var subMenuPermission = new SubMenuPermission
                {
                    SubMenuId = subMenu.Id,
                    PermissionId = permissionId
                };
                await _unitOfWork.Repository<SubMenuPermission>().AddAsync(subMenuPermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(subMenu.Id, "SubMenu created successfully");
    }
}
