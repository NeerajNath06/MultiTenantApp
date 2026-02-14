using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Incidents.Queries.GetIncidentList;

public class GetIncidentListQueryHandler : IRequestHandler<GetIncidentListQuery, ApiResponse<IncidentListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetIncidentListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<IncidentListResponseDto>> Handle(GetIncidentListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<IncidentListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var incidentRepo = _unitOfWork.Repository<IncidentReport>();
        var query = incidentRepo.GetQueryable()
            .Where(i => i.TenantId == _tenantContext.TenantId.Value);

        if (request.SiteId.HasValue)
            query = query.Where(i => i.SiteId == request.SiteId.Value);
        if (request.GuardId.HasValue)
            query = query.Where(i => i.GuardId == request.GuardId.Value);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(i => i.Status.ToString() == request.Status);
        if (!string.IsNullOrEmpty(request.Severity))
            query = query.Where(i => i.Severity.ToString() == request.Severity);
        if (request.StartDate.HasValue)
            query = query.Where(i => i.IncidentDate >= request.StartDate.Value);
        if (request.EndDate.HasValue)
            query = query.Where(i => i.IncidentDate <= request.EndDate.Value);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(i =>
                i.IncidentNumber.ToLower().Contains(search) ||
                i.IncidentType.ToLower().Contains(search) ||
                i.Description.ToLower().Contains(search));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "date" => request.SortDirection == "desc"
                ? query.OrderByDescending(i => i.IncidentDate)
                : query.OrderBy(i => i.IncidentDate),
            "number" => request.SortDirection == "desc"
                ? query.OrderByDescending(i => i.IncidentNumber)
                : query.OrderBy(i => i.IncidentNumber),
            "type" => request.SortDirection == "desc"
                ? query.OrderByDescending(i => i.IncidentType)
                : query.OrderBy(i => i.IncidentType),
            "severity" => request.SortDirection == "desc"
                ? query.OrderByDescending(i => i.Severity)
                : query.OrderBy(i => i.Severity),
            "status" => request.SortDirection == "desc"
                ? query.OrderByDescending(i => i.Status)
                : query.OrderBy(i => i.Status),
            _ => query.OrderByDescending(i => i.IncidentDate)
        };

        var totalCount = query.Count();
        var incidents = await incidentRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var siteIds = incidents.Select(i => i.SiteId).Distinct().ToList();
        var guardIds = incidents.Where(i => i.GuardId.HasValue).Select(i => i.GuardId!.Value).Distinct().ToList();

        var sites = siteIds.Any()
            ? await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken)
            : new List<Site>();
        var guards = guardIds.Any()
            ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)
            : new List<SecurityGuard>();

        var items = incidents.Select(i => new IncidentDto
        {
            Id = i.Id,
            IncidentNumber = i.IncidentNumber,
            SiteId = i.SiteId,
            SiteName = sites.FirstOrDefault(s => s.Id == i.SiteId)?.SiteName ?? "",
            GuardId = i.GuardId,
            GuardName = i.GuardId.HasValue ? $"{guards.FirstOrDefault(g => g.Id == i.GuardId)?.FirstName} {guards.FirstOrDefault(g => g.Id == i.GuardId)?.LastName}" : null,
            IncidentDate = i.IncidentDate,
            IncidentType = i.IncidentType,
            Severity = i.Severity.ToString(),
            Status = i.Status.ToString(),
            Description = i.Description.Length > 100 ? i.Description.Substring(0, 100) + "..." : i.Description
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new IncidentListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<IncidentListResponseDto>.SuccessResponse(response);
    }
}
