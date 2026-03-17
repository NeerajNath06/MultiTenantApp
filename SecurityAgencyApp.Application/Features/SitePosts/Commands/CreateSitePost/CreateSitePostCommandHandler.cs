using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.SitePosts.Commands.CreateSitePost;

public class CreateSitePostCommandHandler : IRequestHandler<CreateSitePostCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateSitePostCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateSitePostCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");

        var tenantId = _tenantContext.TenantId.Value;
        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != tenantId)
            return ApiResponse<Guid>.ErrorResponse("Invalid site");

        if (request.BranchId.HasValue)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId.Value, cancellationToken);
            if (branch == null || branch.TenantId != tenantId)
                return ApiResponse<Guid>.ErrorResponse("Invalid branch");
        }

        var repo = _unitOfWork.Repository<SitePost>();
        var normalizedPostCode = request.PostCode.Trim();
        var existing = await repo.FirstOrDefaultAsync(
            p => p.TenantId == tenantId && p.SiteId == request.SiteId && p.PostCode == normalizedPostCode,
            cancellationToken);
        if (existing != null)
            return ApiResponse<Guid>.ErrorResponse("Post code already exists for this site");

        var entity = new SitePost
        {
            TenantId = tenantId,
            SiteId = request.SiteId,
            BranchId = request.BranchId,
            PostCode = normalizedPostCode,
            PostName = request.PostName.Trim(),
            ShiftName = Normalize(request.ShiftName),
            SanctionedStrength = request.SanctionedStrength,
            GenderRequirement = Normalize(request.GenderRequirement),
            SkillRequirement = Normalize(request.SkillRequirement),
            RequiresWeapon = request.RequiresWeapon,
            RelieverRequired = request.RelieverRequired,
            WeeklyOffPattern = Normalize(request.WeeklyOffPattern),
            IsActive = request.IsActive
        };

        await repo.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<Guid>.SuccessResponse(entity.Id);
    }

    private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
