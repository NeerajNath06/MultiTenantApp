using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.PatrolScans.Queries.GetPatrolScanList;

public class GetPatrolScanListQueryHandler : IRequestHandler<GetPatrolScanListQuery, ApiResponse<PatrolScanListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetPatrolScanListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<PatrolScanListResponseDto>> Handle(GetPatrolScanListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<PatrolScanListResponseDto>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<PatrolScan>();
        var query = repo.GetQueryable()
            .Where(s => s.TenantId == _tenantContext.TenantId.Value && s.GuardId == request.GuardId);

        if (request.SiteId.HasValue)
            query = query.Where(s => s.SiteId == request.SiteId.Value);
        if (request.DateFrom.HasValue)
            query = query.Where(s => s.ScannedAt >= request.DateFrom.Value);
        if (request.DateTo.HasValue)
        {
            var end = request.DateTo.Value.Date.AddDays(1);
            query = query.Where(s => s.ScannedAt < end);
        }

        query = query.OrderByDescending(s => s.ScannedAt);
        var totalCount = query.Count();
        var list = await repo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var siteIds = list.Select(s => s.SiteId).Distinct().ToList();
        var sites = siteIds.Count > 0
            ? (await _unitOfWork.Repository<Site>().FindAsync(site => siteIds.Contains(site.Id), cancellationToken)).ToDictionary(s => s.Id)
            : new Dictionary<Guid, Site>();

        var items = list.Select(s => new PatrolScanDto
        {
            Id = s.Id,
            GuardId = s.GuardId,
            SiteId = s.SiteId,
            SiteName = sites.GetValueOrDefault(s.SiteId)?.SiteName,
            ScannedAt = s.ScannedAt,
            LocationName = s.LocationName,
            CheckpointCode = s.CheckpointCode,
            Status = s.Status
        }).ToList();

        var response = new PatrolScanListResponseDto { Items = items, TotalCount = totalCount };
        return ApiResponse<PatrolScanListResponseDto>.SuccessResponse(response, "Patrol scans retrieved");
    }
}
