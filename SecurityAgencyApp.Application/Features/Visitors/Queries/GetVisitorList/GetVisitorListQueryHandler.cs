using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Visitors.Queries.GetVisitorList;

public class GetVisitorListQueryHandler : IRequestHandler<GetVisitorListQuery, ApiResponse<VisitorListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetVisitorListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<VisitorListResponseDto>> Handle(GetVisitorListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<VisitorListResponseDto>.ErrorResponse("Tenant context not found");

        var repo = _unitOfWork.Repository<Visitor>();
        var query = repo.GetQueryable()
            .Where(v => v.TenantId == _tenantContext.TenantId.Value && v.IsActive);

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

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(v =>
                v.VisitorName.ToLower().Contains(search) ||
                (v.PhoneNumber != null && v.PhoneNumber.ToLower().Contains(search)) ||
                (v.Purpose != null && v.Purpose.ToLower().Contains(search)) ||
                (v.HostName != null && v.HostName.ToLower().Contains(search)));
        }

        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "asc"
                ? query.OrderBy(v => v.VisitorName)
                : query.OrderByDescending(v => v.VisitorName),
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

        var items = list.Select(v => new VisitorDto
        {
            Id = v.Id,
            VisitorName = v.VisitorName,
            VisitorType = v.VisitorType,
            CompanyName = v.CompanyName,
            PhoneNumber = v.PhoneNumber,
            Email = v.Email,
            Purpose = v.Purpose,
            HostName = v.HostName,
            SiteId = v.SiteId,
            SiteName = sites.FirstOrDefault(s => s.Id == v.SiteId)?.SiteName,
            GuardId = v.GuardId,
            GuardName = guards.FirstOrDefault(g => g.Id == v.GuardId) != null
                ? $"{guards.FirstOrDefault(g => g.Id == v.GuardId)!.FirstName} {guards.FirstOrDefault(g => g.Id == v.GuardId)!.LastName}".Trim()
                : null,
            EntryTime = v.EntryTime,
            ExitTime = v.ExitTime,
            IdProofType = v.IdProofType,
            IdProofNumber = v.IdProofNumber,
            BadgeNumber = v.BadgeNumber,
        }).ToList();

        var response = new VisitorListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };
        return ApiResponse<VisitorListResponseDto>.SuccessResponse(response, "Visitors retrieved");
    }
}
