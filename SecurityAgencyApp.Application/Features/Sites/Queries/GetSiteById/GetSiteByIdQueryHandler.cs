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

        var siteDto = new SiteDto
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
            Latitude = site.Latitude,
            Longitude = site.Longitude,
            GeofenceRadiusMeters = site.GeofenceRadiusMeters,
            CreatedDate = site.CreatedDate,
            ModifiedDate = site.ModifiedDate
        };

        var siteSupervisors = (await _unitOfWork.Repository<SiteSupervisor>().FindAsync(
            ss => ss.SiteId == request.Id,
            cancellationToken)).ToList();
        siteDto.SupervisorIds = siteSupervisors.Select(ss => ss.UserId).ToList();

        return ApiResponse<SiteDto>.SuccessResponse(siteDto, "Site retrieved successfully");
    }
}
