using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteList;

public class GetSiteListQueryHandler : IRequestHandler<GetSiteListQuery, ApiResponse<SiteListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSiteListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SiteListResponseDto>> Handle(GetSiteListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<SiteListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var siteRepo = _unitOfWork.Repository<Site>();
        var query = siteRepo.GetQueryable()
            .Where(s => s.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || s.IsActive));

        if (request.SupervisorId.HasValue)
        {
            var supervisedSiteIds = (await _unitOfWork.Repository<SiteSupervisor>().FindAsync(
                ss => ss.UserId == request.SupervisorId.Value,
                cancellationToken)).Select(ss => ss.SiteId).Distinct().ToList();
            if (supervisedSiteIds.Count == 0)
                query = query.Where(_ => false);
            else
                query = query.Where(s => supervisedSiteIds.Contains(s.Id));
        }

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s =>
                s.SiteName.ToLower().Contains(search) ||
                s.SiteCode.ToLower().Contains(search) ||
                s.ClientName.ToLower().Contains(search) ||
                s.City.ToLower().Contains(search) ||
                s.State.ToLower().Contains(search) ||
                (s.ContactPerson != null && s.ContactPerson.ToLower().Contains(search)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.SiteName)
                : query.OrderBy(s => s.SiteName),
            "code" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.SiteCode)
                : query.OrderBy(s => s.SiteCode),
            "client" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.ClientName)
                : query.OrderBy(s => s.ClientName),
            "city" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.City)
                : query.OrderBy(s => s.City),
            "created" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.CreatedDate)
                : query.OrderBy(s => s.CreatedDate),
            _ => query.OrderBy(s => s.SiteName)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var sites = await siteRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var siteDtos = new List<SiteDto>();
        foreach (var site in sites)
        {
            var guardCount = await _unitOfWork.Repository<GuardAssignment>().CountAsync(
                ga => ga.SiteId == site.Id && ga.Status == Domain.Enums.AssignmentStatus.Active,
                cancellationToken);

            siteDtos.Add(new SiteDto
            {
                Id = site.Id,
                SiteCode = site.SiteCode,
                SiteName = site.SiteName,
                ClientName = site.ClientName,
                Address = site.Address,
                City = site.City,
                State = site.State,
                PinCode = site.PinCode,
                ContactPerson = site.ContactPerson,
                ContactPhone = site.ContactPhone,
                ContactEmail = site.ContactEmail,
                IsActive = site.IsActive,
                GuardCount = guardCount,
                Latitude = site.Latitude,
                Longitude = site.Longitude
            });
        }

        var response = new SiteListResponseDto
        {
            Items = siteDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<SiteListResponseDto>.SuccessResponse(response);
    }
}
