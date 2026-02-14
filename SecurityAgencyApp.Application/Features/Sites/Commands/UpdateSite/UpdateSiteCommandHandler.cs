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

        site.SiteCode = request.SiteCode;
        site.SiteName = request.SiteName;
        site.ClientName = request.ClientName;
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
        site.ModifiedDate = DateTime.UtcNow;

        await siteRepo.UpdateAsync(site, cancellationToken);

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

        return ApiResponse<bool>.SuccessResponse(true, "Site updated successfully");
    }
}
