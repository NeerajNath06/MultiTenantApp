using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteById;

public class GetSiteByIdQueryHandler : IRequestHandler<GetSiteByIdQuery, ApiResponse<SiteDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetSiteByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<SiteDto>> Handle(GetSiteByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<SiteDto>.ErrorResponse("Tenant context not found");
        }

        var siteRepo = _unitOfWork.Repository<Site>();
        var site = await siteRepo.GetByIdAsync(request.Id, cancellationToken);

        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<SiteDto>.ErrorResponse("Site not found");
        }

        var branchName = default(string);
        if (site.BranchId.HasValue)
        {
            var branch = await _unitOfWork.Repository<Branch>().GetByIdAsync(site.BranchId.Value, cancellationToken);
            if (branch?.TenantId == _tenantContext.TenantId.Value)
                branchName = branch.BranchName;
        }

        var siteDto = new SiteDto
        {
            Id = site.Id,
            SiteCode = site.SiteCode,
            SiteName = site.SiteName,
            ClientId = site.ClientId,
            BranchName = branchName,
            ClientName = site.ClientName,
            Address = site.Address,
            City = site.City,
            State = site.State,
            PinCode = site.PinCode,
            ContactPerson = site.ContactPerson,
            ContactPhone = site.ContactPhone,
            ContactEmail = site.ContactEmail,
            IsActive = site.IsActive,
            Latitude = site.Latitude,
            Longitude = site.Longitude,
            GeofenceRadiusMeters = site.GeofenceRadiusMeters,
            BranchId = site.BranchId,
            EmergencyContactName = site.EmergencyContactName,
            EmergencyContactPhone = site.EmergencyContactPhone,
            MusterPoint = site.MusterPoint,
            AccessZoneNotes = site.AccessZoneNotes,
            SiteInstructionBook = site.SiteInstructionBook,
            GeofenceExceptionNotes = site.GeofenceExceptionNotes,
            CreatedDate = site.CreatedDate,
            ModifiedDate = site.ModifiedDate
        };

        var siteSupervisors = (await _unitOfWork.Repository<SiteSupervisor>().FindAsync(
            ss => ss.SiteId == request.Id,
            cancellationToken)).ToList();
        siteDto.SupervisorIds = siteSupervisors.Select(ss => ss.UserId).ToList();

        siteDto.Posts = (await _unitOfWork.Repository<SitePost>().FindAsync(
            p => p.SiteId == request.Id,
            cancellationToken))
            .Select(p => new SitePostDto
            {
                Id = p.Id,
                PostCode = p.PostCode,
                PostName = p.PostName,
                ShiftName = p.ShiftName,
                SanctionedStrength = p.SanctionedStrength,
                GenderRequirement = p.GenderRequirement,
                SkillRequirement = p.SkillRequirement,
                RequiresWeapon = p.RequiresWeapon,
                RelieverRequired = p.RelieverRequired,
                WeeklyOffPattern = p.WeeklyOffPattern,
                IsActive = p.IsActive
            })
            .OrderBy(p => p.PostCode)
            .ToList();

        siteDto.ActiveDeploymentPlan = (await _unitOfWork.Repository<SiteDeploymentPlan>().FindAsync(
            p => p.SiteId == request.Id && p.IsActive,
            cancellationToken))
            .OrderByDescending(p => p.EffectiveFrom)
            .Select(p => new SiteDeploymentPlanDto
            {
                Id = p.Id,
                EffectiveFrom = p.EffectiveFrom,
                EffectiveTo = p.EffectiveTo,
                ReservePoolMapping = p.ReservePoolMapping,
                AccessZones = p.AccessZones,
                EmergencyContactSet = p.EmergencyContactSet,
                InstructionSummary = p.InstructionSummary,
                Notes = p.Notes,
                IsActive = p.IsActive
            })
            .FirstOrDefault();

        return ApiResponse<SiteDto>.SuccessResponse(siteDto, "Site retrieved successfully");
    }
}
