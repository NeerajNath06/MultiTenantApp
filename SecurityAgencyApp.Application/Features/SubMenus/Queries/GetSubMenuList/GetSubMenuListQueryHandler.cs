using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SubMenus.Queries.GetSubMenuList;

public class GetSubMenuListQueryHandler : IRequestHandler<GetSubMenuListQuery, ApiResponse<SubMenuListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSubMenuListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SubMenuListResponseDto>> Handle(GetSubMenuListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<SubMenuListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var subMenuRepo = _unitOfWork.Repository<SubMenu>();
        var menuRepo = _unitOfWork.Repository<Menu>();

        var query = subMenuRepo.FindAsync(
            sm => sm.TenantId == _tenantContext.TenantId.Value &&
                 (request.MenuId == null || sm.MenuId == request.MenuId.Value) &&
                 (request.IncludeInactive || sm.IsActive),
            cancellationToken);

        var subMenus = await query;
        var allMenus = await menuRepo.FindAsync(m => m.TenantId == _tenantContext.TenantId.Value, cancellationToken);

        var items = subMenus
            .OrderBy(sm => sm.DisplayOrder)
            .Select(sm => new SubMenuDto
            {
                Id = sm.Id,
                MenuId = sm.MenuId,
                MenuName = allMenus.FirstOrDefault(m => m.Id == sm.MenuId)?.Name ?? "Unknown",
                Name = sm.Name,
                DisplayName = sm.DisplayName,
                Icon = sm.Icon,
                Route = sm.Route,
                DisplayOrder = sm.DisplayOrder,
                IsActive = sm.IsActive
            })
            .ToList();

        var response = new SubMenuListResponseDto
        {
            Items = items,
            TotalCount = items.Count
        };

        return ApiResponse<SubMenuListResponseDto>.SuccessResponse(response);
    }
}
