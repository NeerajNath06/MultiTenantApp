using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentList;

public class GetEquipmentListQueryHandler : IRequestHandler<GetEquipmentListQuery, ApiResponse<EquipmentListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetEquipmentListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<EquipmentListResponseDto>> Handle(GetEquipmentListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<EquipmentListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var equipmentRepo = _unitOfWork.Repository<Domain.Entities.Equipment>();
        var query = equipmentRepo.GetQueryable()
            .Where(e => e.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || e.IsActive));

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(e =>
                e.EquipmentCode.ToLower().Contains(search) ||
                e.EquipmentName.ToLower().Contains(search) ||
                (e.SerialNumber != null && e.SerialNumber.ToLower().Contains(search)));
        }

        if (!string.IsNullOrWhiteSpace(request.Category))
        {
            query = query.Where(e => e.Category == request.Category);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        if (request.AssignedToGuardId.HasValue)
        {
            query = query.Where(e => e.AssignedToGuardId == request.AssignedToGuardId.Value);
        }

        if (request.AssignedToSiteId.HasValue)
        {
            query = query.Where(e => e.AssignedToSiteId == request.AssignedToSiteId.Value);
        }

        query = request.SortBy?.ToLower() switch
        {
            "equipmentcode" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.EquipmentCode)
                : query.OrderByDescending(e => e.EquipmentCode),
            "equipmentname" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.EquipmentName)
                : query.OrderByDescending(e => e.EquipmentName),
            "purchasedate" => request.SortDirection == "asc"
                ? query.OrderBy(e => e.PurchaseDate)
                : query.OrderByDescending(e => e.PurchaseDate),
            _ => query.OrderBy(e => e.EquipmentCode)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var equipment = await equipmentRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get related data
        var guardIds = equipment.Where(e => e.AssignedToGuardId.HasValue).Select(e => e.AssignedToGuardId!.Value).Distinct().ToList();
        var siteIds = equipment.Where(e => e.AssignedToSiteId.HasValue).Select(e => e.AssignedToSiteId!.Value).Distinct().ToList();

        var guards = guardIds.Any()
            ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)
            : new List<SecurityGuard>();

        var sites = siteIds.Any()
            ? await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)
            : new List<Site>();

        var equipmentDtos = equipment.Select(e => new EquipmentDto
        {
            Id = e.Id,
            EquipmentCode = e.EquipmentCode,
            EquipmentName = e.EquipmentName,
            Category = e.Category,
            Manufacturer = e.Manufacturer,
            ModelNumber = e.ModelNumber,
            PurchaseDate = e.PurchaseDate,
            PurchaseCost = e.PurchaseCost,
            Status = e.Status,
            AssignedToGuardId = e.AssignedToGuardId,
            AssignedToGuardName = e.AssignedToGuardId.HasValue
                ? $"{guards.FirstOrDefault(g => g.Id == e.AssignedToGuardId.Value)?.FirstName} {guards.FirstOrDefault(g => g.Id == e.AssignedToGuardId.Value)?.LastName}"
                : null,
            AssignedToSiteId = e.AssignedToSiteId,
            AssignedToSiteName = e.AssignedToSiteId.HasValue ? sites.FirstOrDefault(s => s.Id == e.AssignedToSiteId.Value)?.SiteName : null,
            NextMaintenanceDate = e.NextMaintenanceDate,
            IsActive = e.IsActive,
            CreatedDate = e.CreatedDate
        }).ToList();

        var response = new EquipmentListResponseDto
        {
            Items = equipmentDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<EquipmentListResponseDto>.SuccessResponse(response, "Equipment retrieved successfully");
    }
}
