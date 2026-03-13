using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SitePosts;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostList;

public class GetSitePostListQueryHandler : IRequestHandler<GetSitePostListQuery, ApiResponse<SitePostListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSitePostListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SitePostListResponseDto>> Handle(GetSitePostListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<SitePostListResponseDto>.ErrorResponse("Tenant context not found");

        var tenantId = _tenantContext.TenantId.Value;
        var query = _unitOfWork.Repository<SitePost>().GetQueryable()
            .Where(p => p.TenantId == tenantId && (request.IncludeInactive || p.IsActive));

        if (request.SiteId.HasValue)
            query = query.Where(p => p.SiteId == request.SiteId.Value);

        if (request.BranchId.HasValue)
            query = query.Where(p => p.BranchId == request.BranchId.Value);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim().ToLower();
            query = query.Where(p =>
                p.PostCode.ToLower().Contains(search) ||
                p.PostName.ToLower().Contains(search) ||
                (p.ShiftName != null && p.ShiftName.ToLower().Contains(search)) ||
                (p.GenderRequirement != null && p.GenderRequirement.ToLower().Contains(search)) ||
                (p.SkillRequirement != null && p.SkillRequirement.ToLower().Contains(search)));
        }

        query = query.OrderBy(p => p.SiteId).ThenBy(p => p.PostCode);
        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var posts = await _unitOfWork.Repository<SitePost>().GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var siteIds = posts.Select(p => p.SiteId).Distinct().ToList();
        var branchIds = posts.Where(p => p.BranchId.HasValue).Select(p => p.BranchId!.Value).Distinct().ToList();

        var sites = siteIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken))
                .ToDictionary(s => s.Id, s => s.SiteName);

        var branches = branchIds.Count == 0
            ? new Dictionary<Guid, string>()
            : (await _unitOfWork.Repository<Branch>().FindAsync(b => branchIds.Contains(b.Id), cancellationToken))
                .ToDictionary(b => b.Id, b => b.BranchName);

        var items = posts.Select(p => new SitePostDto
        {
            Id = p.Id,
            SiteId = p.SiteId,
            SiteName = sites.TryGetValue(p.SiteId, out var siteName) ? siteName : string.Empty,
            BranchId = p.BranchId,
            BranchName = p.BranchId.HasValue && branches.TryGetValue(p.BranchId.Value, out var branchName) ? branchName : null,
            PostCode = p.PostCode,
            PostName = p.PostName,
            ShiftName = p.ShiftName,
            SanctionedStrength = p.SanctionedStrength,
            GenderRequirement = p.GenderRequirement,
            SkillRequirement = p.SkillRequirement,
            RequiresWeapon = p.RequiresWeapon,
            RelieverRequired = p.RelieverRequired,
            WeeklyOffPattern = p.WeeklyOffPattern,
            IsActive = p.IsActive,
            CreatedDate = p.CreatedDate,
            ModifiedDate = p.ModifiedDate
        }).ToList();

        return ApiResponse<SitePostListResponseDto>.SuccessResponse(new SitePostListResponseDto
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            Search = request.Search,
            SiteId = request.SiteId,
            BranchId = request.BranchId
        }, "Site posts retrieved successfully");
    }
}
