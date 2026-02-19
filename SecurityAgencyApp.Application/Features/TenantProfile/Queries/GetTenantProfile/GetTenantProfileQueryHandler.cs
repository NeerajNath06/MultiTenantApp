using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;

public class GetTenantProfileQueryHandler : IRequestHandler<GetTenantProfileQuery, ApiResponse<TenantProfileDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetTenantProfileQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<TenantProfileDto>> Handle(GetTenantProfileQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<TenantProfileDto>.ErrorResponse("Tenant context not found");

        var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(_tenantContext.TenantId.Value, cancellationToken);
        if (tenant == null)
            return ApiResponse<TenantProfileDto>.ErrorResponse("Tenant not found");

        var dto = new TenantProfileDto
        {
            Id = tenant.Id,
            CompanyName = tenant.CompanyName,
            RegistrationNumber = tenant.RegistrationNumber,
            Email = tenant.Email,
            Phone = tenant.Phone,
            Address = tenant.Address,
            City = tenant.City,
            State = tenant.State,
            Country = tenant.Country,
            PinCode = tenant.PinCode,
            Website = tenant.Website,
            TaxId = tenant.TaxId,
            LogoPath = tenant.LogoPath,
            IsActive = tenant.IsActive,
            SubscriptionStartDate = tenant.SubscriptionStartDate,
            SubscriptionEndDate = tenant.SubscriptionEndDate
        };
        return ApiResponse<TenantProfileDto>.SuccessResponse(dto, "Profile retrieved successfully");
    }
}
