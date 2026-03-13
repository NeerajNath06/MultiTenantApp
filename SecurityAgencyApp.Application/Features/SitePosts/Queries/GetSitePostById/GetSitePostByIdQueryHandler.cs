using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SitePosts;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostById;

public class GetSitePostByIdQueryHandler : IRequestHandler<GetSitePostByIdQuery, ApiResponse<SitePostDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSitePostByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SitePostDto>> Handle(GetSitePostByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<SitePostDto>.ErrorResponse("Tenant context not found");

        var tenantId = _tenantContext.TenantId.Value;
        var entity = await _unitOfWork.Repository<SitePost>().GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != tenantId)
            return ApiResponse<SitePostDto>.ErrorResponse("Site post not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(entity.SiteId, cancellationToken);
        var branch = entity.BranchId.HasValue
            ? await _unitOfWork.Repository<Branch>().GetByIdAsync(entity.BranchId.Value, cancellationToken)
            : null;

        return ApiResponse<SitePostDto>.SuccessResponse(new SitePostDto
        {
            Id = entity.Id,
            SiteId = entity.SiteId,
            SiteName = site?.TenantId == tenantId ? site.SiteName : string.Empty,
            BranchId = entity.BranchId,
            BranchName = branch?.TenantId == tenantId ? branch.BranchName : null,
            PostCode = entity.PostCode,
            PostName = entity.PostName,
            ShiftName = entity.ShiftName,
            SanctionedStrength = entity.SanctionedStrength,
            GenderRequirement = entity.GenderRequirement,
            SkillRequirement = entity.SkillRequirement,
            RequiresWeapon = entity.RequiresWeapon,
            RelieverRequired = entity.RelieverRequired,
            WeeklyOffPattern = entity.WeeklyOffPattern,
            IsActive = entity.IsActive,
            CreatedDate = entity.CreatedDate,
            ModifiedDate = entity.ModifiedDate
        }, "Site post retrieved successfully");
    }
}
