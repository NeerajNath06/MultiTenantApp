using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuById;

public class GetMenuByIdQueryHandler : IRequestHandler<GetMenuByIdQuery, ApiResponse<MenuDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetMenuByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<MenuDto>> Handle(GetMenuByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<MenuDto>.ErrorResponse("Tenant context not found");
        }

        var menuRepo = _unitOfWork.Repository<Menu>();
        var menu = await menuRepo.GetByIdAsync(request.Id, cancellationToken);

        if (menu == null || menu.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<MenuDto>.ErrorResponse("Menu not found");
        }

        // Get submenus
        var subMenus = await _unitOfWork.Repository<SubMenu>().FindAsync(
            sm => sm.MenuId == menu.Id, cancellationToken);

        // Get permissions
        var menuPermissions = await _unitOfWork.Repository<MenuPermission>().FindAsync(
            mp => mp.MenuId == menu.Id, cancellationToken);

        var menuDto = new MenuDto
        {
            Id = menu.Id,
            Name = menu.Name,
            DisplayName = menu.DisplayName,
            Icon = menu.Icon,
            Route = menu.Route,
            DisplayOrder = menu.DisplayOrder,
            IsActive = menu.IsActive,
            SubMenus = subMenus
                .OrderBy(sm => sm.DisplayOrder)
                .Select(sm => new SubMenuDto
                {
                    Id = sm.Id,
                    Name = sm.Name,
                    DisplayName = sm.DisplayName,
                    Icon = sm.Icon,
                    Route = sm.Route,
                    DisplayOrder = sm.DisplayOrder,
                    IsActive = sm.IsActive
                })
                .ToList(),
            PermissionIds = menuPermissions.Select(mp => mp.PermissionId).ToList()
        };

        return ApiResponse<MenuDto>.SuccessResponse(menuDto, "Menu retrieved successfully");
    }
}
