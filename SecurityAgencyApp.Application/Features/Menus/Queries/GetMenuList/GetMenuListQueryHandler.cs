using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Menus.Queries.GetMenuList;

public class GetMenuListQueryHandler : IRequestHandler<GetMenuListQuery, ApiResponse<MenuListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetMenuListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<MenuListResponseDto>> Handle(GetMenuListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<MenuListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var menuRepo = _unitOfWork.Repository<Menu>();
        var query = menuRepo.GetQueryable()
            .Where(m => m.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || m.IsActive));

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(m =>
                m.Name.ToLower().Contains(search) ||
                (m.DisplayName != null && m.DisplayName.ToLower().Contains(search)) ||
                (m.Route != null && m.Route.ToLower().Contains(search)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(m => m.Name)
                : query.OrderBy(m => m.Name),
            "order" => request.SortDirection == "desc"
                ? query.OrderByDescending(m => m.DisplayOrder)
                : query.OrderBy(m => m.DisplayOrder),
            _ => query.OrderBy(m => m.DisplayOrder)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var menus = await menuRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get submenus for all menus
        var menuIds = menus.Select(m => m.Id).ToList();
        var subMenuRepo = _unitOfWork.Repository<SubMenu>();
        var allSubMenus = await subMenuRepo.FindAsync(
            sm => menuIds.Contains(sm.MenuId) && (request.IncludeInactive || sm.IsActive),
            cancellationToken);

        var menuDtos = menus.Select(m => new MenuDto
        {
            Id = m.Id,
            Name = m.Name,
            DisplayName = m.DisplayName,
            Icon = m.Icon,
            Route = m.Route,
            DisplayOrder = m.DisplayOrder,
            IsActive = m.IsActive,
            SubMenus = allSubMenus
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

        var response = new MenuListResponseDto
        {
            Items = menuDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<MenuListResponseDto>.SuccessResponse(response, "Menus retrieved successfully");
    }
}
