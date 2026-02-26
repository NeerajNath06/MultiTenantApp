using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.VehicleLogs.Queries.GetVehicleLogList;

public class GetVehicleLogListQueryHandler : IRequestHandler<GetVehicleLogListQuery, ApiResponse<VehicleLogListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetVehicleLogListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<VehicleLogListResponseDto>> Handle(GetVehicleLogListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<VehicleLogListResponseDto>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<VehicleLog>();
        var query = repo.GetQueryable()
            .Where(v => v.TenantId == _tenantContext.TenantId.Value);

        if (request.SiteId.HasValue)
            query = query.Where(v => v.SiteId == request.SiteId.Value);
        if (request.GuardId.HasValue)
            query = query.Where(v => v.GuardId == request.GuardId.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(v => v.EntryTime >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
        {
            var end = request.DateTo.Value.Date.AddDays(1);
            query = query.Where(v => v.EntryTime < end);
        }
        if (request.InsideOnly == true)
            query = query.Where(v => v.ExitTime == null);
        if (request.InsideOnly == false)
            query = query.Where(v => v.ExitTime != null);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(v =>
                v.VehicleNumber.ToLower().Contains(search) ||
                v.DriverName.ToLower().Contains(search) ||
                (v.Purpose != null && v.Purpose.ToLower().Contains(search)) ||
                (v.DriverPhone != null && v.DriverPhone.Contains(search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "vehiclenumber" => request.SortDirection == "asc"
                ? query.OrderBy(v => v.VehicleNumber)
                : query.OrderByDescending(v => v.VehicleNumber),
            "entrytime" => request.SortDirection == "asc"
                ? query.OrderBy(v => v.EntryTime)
                : query.OrderByDescending(v => v.EntryTime),
            "purpose" => request.SortDirection == "asc"
                ? query.OrderBy(v => v.Purpose)
                : query.OrderByDescending(v => v.Purpose),
            _ => query.OrderByDescending(v => v.EntryTime)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var list = await repo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var siteIds = list.Select(v => v.SiteId).Distinct().ToList();
        var guardIds = list.Select(v => v.GuardId).Distinct().ToList();
        var sites = siteIds.Any() ? await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken) : new List<Site>();
        var guards = guardIds.Any() ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken) : new List<SecurityGuard>();

        var items = list.Select(v => new VehicleLogDto
        {
            Id = v.Id,
            VehicleNumber = v.VehicleNumber,
            VehicleType = v.VehicleType,
            DriverName = v.DriverName,
            DriverPhone = v.DriverPhone,
            Purpose = v.Purpose,
            ParkingSlot = v.ParkingSlot,
            SiteId = v.SiteId,
            SiteName = sites.FirstOrDefault(s => s.Id == v.SiteId)?.SiteName,
            GuardId = v.GuardId,
            GuardName = guards.FirstOrDefault(g => g.Id == v.GuardId) != null
                ? $"{guards.FirstOrDefault(g => g.Id == v.GuardId)!.FirstName} {guards.FirstOrDefault(g => g.Id == v.GuardId)!.LastName}".Trim()
                : null,
            EntryTime = v.EntryTime,
            ExitTime = v.ExitTime,
        }).ToList();

        var response = new VehicleLogListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
        return ApiResponse<VehicleLogListResponseDto>.SuccessResponse(response, "Vehicle log retrieved");
    }
}
