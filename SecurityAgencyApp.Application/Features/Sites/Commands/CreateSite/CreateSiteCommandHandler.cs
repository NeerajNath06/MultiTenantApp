using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Sites.Commands.CreateSite;

public class CreateSiteCommandHandler : IRequestHandler<CreateSiteCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateSiteCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateSiteCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        var siteRepo = _unitOfWork.Repository<Site>();
        var existing = await siteRepo.FirstOrDefaultAsync(
            s => s.SiteCode == request.SiteCode && s.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Site code already exists");
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId.Value, cancellationToken);
            if (branch == null || branch.TenantId != _tenantContext.TenantId.Value)
                return ApiResponse<Guid>.ErrorResponse("Invalid branch");
        }

        var site = new Site
        {
            TenantId = _tenantContext.TenantId.Value,
            SiteCode = request.SiteCode,
            SiteName = request.SiteName,
            ClientName = request.ClientName,
            Address = request.Address,
            City = request.City,
            State = request.State,
            PinCode = request.PinCode,
            ContactPerson = request.ContactPerson,
            ContactPhone = request.ContactPhone,
            ContactEmail = request.ContactEmail,
            IsActive = request.IsActive,
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            GeofenceRadiusMeters = request.GeofenceRadiusMeters,
            BranchId = request.BranchId,
            EmergencyContactName = request.EmergencyContactName,
            EmergencyContactPhone = request.EmergencyContactPhone,
            MusterPoint = request.MusterPoint,
            AccessZoneNotes = request.AccessZoneNotes,
            SiteInstructionBook = request.SiteInstructionBook,
            GeofenceExceptionNotes = request.GeofenceExceptionNotes
        };

        await siteRepo.AddAsync(site, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SyncSitePlanningAsync(site.Id, request.BranchId, request.Posts, request.DeploymentPlan, cancellationToken);

        if (request.SupervisorIds != null && request.SupervisorIds.Count > 0)
        {
            var ssRepo = _unitOfWork.Repository<SiteSupervisor>();
            foreach (var userId in request.SupervisorIds.Distinct())
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
                if (user != null && user.TenantId == _tenantContext.TenantId.Value)
                {
                    await ssRepo.AddAsync(new SiteSupervisor
                    {
                        SiteId = site.Id,
                        UserId = userId,
                        TenantId = _tenantContext.TenantId.Value,
                        CreatedDate = DateTime.UtcNow
                    }, cancellationToken);
                }
            }
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return ApiResponse<Guid>.SuccessResponse(site.Id, "Site created successfully");
    }

    private async Task SyncSitePlanningAsync(
        Guid siteId,
        Guid? branchId,
        List<SitePostInputDto>? posts,
        SiteDeploymentPlanInputDto? deploymentPlan,
        CancellationToken cancellationToken)
    {
        var postRepo = _unitOfWork.Repository<SitePost>();
        foreach (var post in posts ?? new List<SitePostInputDto>())
        {
            if (string.IsNullOrWhiteSpace(post.PostCode) || string.IsNullOrWhiteSpace(post.PostName))
                continue;

            await postRepo.AddAsync(new SitePost
            {
                TenantId = _tenantContext.TenantId!.Value,
                SiteId = siteId,
                BranchId = branchId,
                PostCode = post.PostCode.Trim(),
                PostName = post.PostName.Trim(),
                ShiftName = string.IsNullOrWhiteSpace(post.ShiftName) ? null : post.ShiftName.Trim(),
                SanctionedStrength = post.SanctionedStrength,
                GenderRequirement = string.IsNullOrWhiteSpace(post.GenderRequirement) ? null : post.GenderRequirement.Trim(),
                SkillRequirement = string.IsNullOrWhiteSpace(post.SkillRequirement) ? null : post.SkillRequirement.Trim(),
                RequiresWeapon = post.RequiresWeapon,
                RelieverRequired = post.RelieverRequired,
                WeeklyOffPattern = string.IsNullOrWhiteSpace(post.WeeklyOffPattern) ? null : post.WeeklyOffPattern.Trim(),
                IsActive = post.IsActive
            }, cancellationToken);
        }

        if (deploymentPlan != null && deploymentPlan.EffectiveFrom != default)
        {
            await _unitOfWork.Repository<SiteDeploymentPlan>().AddAsync(new SiteDeploymentPlan
            {
                TenantId = _tenantContext.TenantId!.Value,
                SiteId = siteId,
                BranchId = branchId,
                EffectiveFrom = deploymentPlan.EffectiveFrom,
                EffectiveTo = deploymentPlan.EffectiveTo,
                ReservePoolMapping = string.IsNullOrWhiteSpace(deploymentPlan.ReservePoolMapping) ? null : deploymentPlan.ReservePoolMapping.Trim(),
                AccessZones = string.IsNullOrWhiteSpace(deploymentPlan.AccessZones) ? null : deploymentPlan.AccessZones.Trim(),
                EmergencyContactSet = string.IsNullOrWhiteSpace(deploymentPlan.EmergencyContactSet) ? null : deploymentPlan.EmergencyContactSet.Trim(),
                InstructionSummary = string.IsNullOrWhiteSpace(deploymentPlan.InstructionSummary) ? null : deploymentPlan.InstructionSummary.Trim(),
                Notes = string.IsNullOrWhiteSpace(deploymentPlan.Notes) ? null : deploymentPlan.Notes.Trim(),
                IsActive = deploymentPlan.IsActive
            }, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
