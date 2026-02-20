using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuList;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Queries.GetMenusForCurrentUser;

public class GetMenusForCurrentUserQueryHandler : IRequestHandler<GetMenusForCurrentUserQuery, ApiResponse<GetMenusForCurrentUserResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public GetMenusForCurrentUserQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<GetMenusForCurrentUserResponseDto>> Handle(GetMenusForCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated || !_currentUserService.UserId.HasValue)
        {
            return ApiResponse<GetMenusForCurrentUserResponseDto>.SuccessResponse(
                new GetMenusForCurrentUserResponseDto { Items = new List<MenuDto>() }, "No menus for unauthenticated user");
        }

        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<GetMenusForCurrentUserResponseDto>.ErrorResponse("Tenant context not found");
        }

        var userId = _currentUserService.UserId.Value;

        // Get user's role ids
        var userRoles = await _unitOfWork.Repository<UserRole>().FindAsync(
            ur => ur.UserId == userId, cancellationToken);
        var roleIds = userRoles.Select(ur => ur.RoleId).Distinct().ToList();

        if (roleIds.Count == 0)
        {
            return ApiResponse<GetMenusForCurrentUserResponseDto>.SuccessResponse(
                new GetMenusForCurrentUserResponseDto { Items = new List<MenuDto>() }, "No roles assigned");
        }

        var tenantId = _tenantContext.TenantId.Value;

        // Allowed menu and submenu ids for these roles
        var roleMenus = await _unitOfWork.Repository<RoleMenu>().FindAsync(
            rm => roleIds.Contains(rm.RoleId), cancellationToken);
        var roleSubMenus = await _unitOfWork.Repository<RoleSubMenu>().FindAsync(
            rsm => roleIds.Contains(rsm.RoleId), cancellationToken);

        var allowedMenuIds = roleMenus.Select(rm => rm.MenuId).Distinct().ToHashSet();
        var allowedSubMenuIds = roleSubMenus.Select(rsm => rsm.SubMenuId).Distinct().ToHashSet();

        if (allowedMenuIds.Count == 0)
        {
            return ApiResponse<GetMenusForCurrentUserResponseDto>.SuccessResponse(
                new GetMenusForCurrentUserResponseDto { Items = new List<MenuDto>() }, "No menus assigned to your roles");
        }

        var menuRepo = _unitOfWork.Repository<Menu>();
        var menus = await menuRepo.FindAsync(
            m => m.TenantId == tenantId && m.IsActive && allowedMenuIds.Contains(m.Id),
            cancellationToken);
        menus = menus.OrderBy(m => m.DisplayOrder).ToList();

        var menuIds = menus.Select(m => m.Id).ToList();
        var subMenuRepo = _unitOfWork.Repository<SubMenu>();
        var subMenus = await subMenuRepo.FindAsync(
            sm => menuIds.Contains(sm.MenuId) && sm.IsActive && allowedSubMenuIds.Contains(sm.Id),
            cancellationToken);

        var items = menus.Select(m => new MenuDto
        {
            Id = m.Id,
            Name = m.Name,
            DisplayName = m.DisplayName,
            Icon = m.Icon,
            Route = m.Route,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive,
            SubMenus = subMenus
                .Where(sm => sm.MenuId == m.Id)
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
                .ToList()
        }).ToList();

        return ApiResponse<GetMenusForCurrentUserResponseDto>.SuccessResponse(
            new GetMenusForCurrentUserResponseDto { Items = items }, "Menus retrieved successfully");
    }
}
