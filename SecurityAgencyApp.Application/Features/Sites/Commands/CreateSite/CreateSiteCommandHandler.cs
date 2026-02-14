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
            GeofenceRadiusMeters = request.GeofenceRadiusMeters
        };

        await siteRepo.AddAsync(site, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

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
}
