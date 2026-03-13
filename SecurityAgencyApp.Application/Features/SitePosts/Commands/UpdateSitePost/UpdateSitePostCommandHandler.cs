using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SitePosts.Commands.UpdateSitePost;

public class UpdateSitePostCommandHandler : IRequestHandler<UpdateSitePostCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateSitePostCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateSitePostCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var tenantId = _tenantContext.TenantId.Value;
        var repo = _unitOfWork.Repository<SitePost>();
        var entity = await repo.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null || entity.TenantId != tenantId)
            return ApiResponse<bool>.ErrorResponse("Site post not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != tenantId)
            return ApiResponse<bool>.ErrorResponse("Invalid site");

        if (request.BranchId.HasValue)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId.Value, cancellationToken);
            if (branch == null || branch.TenantId != tenantId)
                return ApiResponse<bool>.ErrorResponse("Invalid branch");
        }

        var normalizedPostCode = request.PostCode.Trim();
        var existing = await repo.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.SiteId == request.SiteId && p.PostCode == normalizedPostCode && p.Id != request.Id,
            cancellationToken);
        if (existing != null)
            return ApiResponse<bool>.ErrorResponse("Post code already exists for this site");

        entity.SiteId = request.SiteId;
        entity.BranchId = request.BranchId;
        entity.PostCode = normalizedPostCode;
        entity.PostName = request.PostName.Trim();
        entity.ShiftName = Normalize(request.ShiftName);
        entity.SanctionedStrength = request.SanctionedStrength;
        entity.GenderRequirement = Normalize(request.GenderRequirement);
        entity.SkillRequirement = Normalize(request.SkillRequirement);
        entity.RequiresWeapon = request.RequiresWeapon;
        entity.RelieverRequired = request.RelieverRequired;
        entity.WeeklyOffPattern = Normalize(request.WeeklyOffPattern);
        entity.IsActive = request.IsActive;
        entity.ModifiedDate = DateTime.UtcNow;

        await repo.UpdateAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true, "Site post updated successfully");
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
