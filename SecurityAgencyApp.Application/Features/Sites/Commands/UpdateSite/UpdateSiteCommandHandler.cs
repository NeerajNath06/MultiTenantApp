using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Sites.Commands.UpdateSite;

public class UpdateSiteCommandHandler : IRequestHandler<UpdateSiteCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateSiteCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateSiteCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");
        }

        var siteRepo = _unitOfWork.Repository<Site>();
        var site = await siteRepo.GetByIdAsync(request.Id, cancellationToken);

        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<bool>.ErrorResponse("Site not found");
        }

        if (request.BranchId.HasValue)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(request.BranchId.Value, cancellationToken);
            if (branch == null || branch.TenantId != _tenantContext.TenantId.Value)
                return ApiResponse<bool>.ErrorResponse("Invalid branch");
        }

        site.SiteCode = request.SiteCode;
        site.SiteName = request.SiteName;
        // Link to client if provided; keep ClientName in sync with Client.CompanyName
        if (request.ClientId.HasValue && request.ClientId.Value != Guid.Empty)
        {
            var client = await _unitOfWork.Repository<Client>().GetByIdAsync(request.ClientId.Value, cancellationToken);
            if (client != null && client.TenantId == _tenantContext.TenantId.Value)
            {
                site.ClientId = client.Id;
                site.ClientName = client.CompanyName;
            }
        }
        else
        {
            site.ClientId = null;
            site.ClientName = request.ClientName;
        }
        site.Address = request.Address;
        site.City = request.City;
        site.State = request.State;
        site.PinCode = request.PinCode;
        site.ContactPerson = request.ContactPerson;
        site.ContactPhone = request.ContactPhone;
        site.ContactEmail = request.ContactEmail;
        site.IsActive = request.IsActive;
        site.Latitude = request.Latitude;
        site.Longitude = request.Longitude;
        site.GeofenceRadiusMeters = request.GeofenceRadiusMeters;
        site.BranchId = request.BranchId;
        site.EmergencyContactName = request.EmergencyContactName;
        site.EmergencyContactPhone = request.EmergencyContactPhone;
        site.MusterPoint = request.MusterPoint;
        site.AccessZoneNotes = request.AccessZoneNotes;
        site.SiteInstructionBook = request.SiteInstructionBook;
        site.GeofenceExceptionNotes = request.GeofenceExceptionNotes;
        site.ModifiedDate = DateTime.UtcNow;

        await siteRepo.UpdateAsync(site, cancellationToken);

        var sitePostRepo = _unitOfWork.Repository<SitePost>();
        var existingPosts = await sitePostRepo.FindAsync(p => p.SiteId == request.Id, cancellationToken);
        foreach (var existingPost in existingPosts)
            await sitePostRepo.DeleteAsync(existingPost, cancellationToken);

        foreach (var post in request.Posts ?? new List<SitePostInputDto>())
        {
            if (string.IsNullOrWhiteSpace(post.PostCode) || string.IsNullOrWhiteSpace(post.PostName))
                continue;

            await sitePostRepo.AddAsync(new SitePost
            {
                TenantId = _tenantContext.TenantId.Value,
                SiteId = request.Id,
                BranchId = request.BranchId,
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

        var deploymentPlanRepo = _unitOfWork.Repository<SiteDeploymentPlan>();
        var existingPlans = await deploymentPlanRepo.FindAsync(p => p.SiteId == request.Id, cancellationToken);
        foreach (var existingPlan in existingPlans)
            await deploymentPlanRepo.DeleteAsync(existingPlan, cancellationToken);

        if (request.DeploymentPlan != null && request.DeploymentPlan.EffectiveFrom != default)
        {
            await deploymentPlanRepo.AddAsync(new SiteDeploymentPlan
            {
                TenantId = _tenantContext.TenantId.Value,
                SiteId = request.Id,
                BranchId = request.BranchId,
                EffectiveFrom = request.DeploymentPlan.EffectiveFrom,
                EffectiveTo = request.DeploymentPlan.EffectiveTo,
                ReservePoolMapping = string.IsNullOrWhiteSpace(request.DeploymentPlan.ReservePoolMapping) ? null : request.DeploymentPlan.ReservePoolMapping.Trim(),
                AccessZones = string.IsNullOrWhiteSpace(request.DeploymentPlan.AccessZones) ? null : request.DeploymentPlan.AccessZones.Trim(),
                EmergencyContactSet = string.IsNullOrWhiteSpace(request.DeploymentPlan.EmergencyContactSet) ? null : request.DeploymentPlan.EmergencyContactSet.Trim(),
                InstructionSummary = string.IsNullOrWhiteSpace(request.DeploymentPlan.InstructionSummary) ? null : request.DeploymentPlan.InstructionSummary.Trim(),
                Notes = string.IsNullOrWhiteSpace(request.DeploymentPlan.Notes) ? null : request.DeploymentPlan.Notes.Trim(),
                IsActive = request.DeploymentPlan.IsActive
            }, cancellationToken);
        }

        if (request.SupervisorIds != null)
        {
            var ssRepo = _unitOfWork.Repository<SiteSupervisor>();
            var existing = (await ssRepo.FindAsync(ss => ss.SiteId == request.Id, cancellationToken)).ToList();
            foreach (var e in existing)
                await ssRepo.DeleteAsync(e, cancellationToken);
            foreach (var userId in request.SupervisorIds.Distinct())
            {
                var user = await _unitOfWork.Repository<User>().GetByIdAsync(userId, cancellationToken);
                if (user != null && user.TenantId == _tenantContext.TenantId.Value)
                {
                    await ssRepo.AddAsync(new SiteSupervisor
                    {
                        SiteId = request.Id,
                        UserId = userId,
                        TenantId = _tenantContext.TenantId.Value,
                        CreatedDate = DateTime.UtcNow
                    }, cancellationToken);
                }
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<bool>.SuccessResponse(true);
    }
}
