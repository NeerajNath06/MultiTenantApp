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
            LegalName = tenant.LegalName,
            TradeName = tenant.TradeName,
            CompanyCode = tenant.CompanyCode,
            CinNumber = tenant.CinNumber,
            GstNumber = tenant.GstNumber,
            PanNumber = tenant.PanNumber,
            PfNumber = tenant.PfNumber,
            EsicNumber = tenant.EsicNumber,
            LabourLicenseNumber = tenant.LabourLicenseNumber,
            OwnerName = tenant.OwnerName,
            ComplianceOfficerName = tenant.ComplianceOfficerName,
            BillingContactName = tenant.BillingContactName,
            BillingContactPhone = tenant.BillingContactPhone,
            BillingEmail = tenant.BillingEmail,
            EscalationContactName = tenant.EscalationContactName,
            EscalationContactPhone = tenant.EscalationContactPhone,
            SupportEmail = tenant.SupportEmail,
            Address = tenant.Address,
            City = tenant.City,
            State = tenant.State,
            Country = tenant.Country,
            PinCode = tenant.PinCode,
            Website = tenant.Website,
            TaxId = tenant.TaxId,
            TimeZone = tenant.TimeZone,
            Currency = tenant.Currency,
            InvoicePrefix = tenant.InvoicePrefix,
            PayrollPrefix = tenant.PayrollPrefix,
            SubscriptionPlan = tenant.SubscriptionPlan,
            SeatLimit = tenant.SeatLimit,
            BranchLimit = tenant.BranchLimit,
            StorageLimitGb = tenant.StorageLimitGb,
            OnboardingStatus = tenant.OnboardingStatus,
            ActivationStatus = tenant.ActivationStatus,
            IsKycVerified = tenant.IsKycVerified,
            OnboardingChecklistCompleted = tenant.OnboardingChecklistCompleted,
            LogoPath = tenant.LogoPath,
            IsActive = tenant.IsActive,
            SubscriptionStartDate = tenant.SubscriptionStartDate,
            SubscriptionEndDate = tenant.SubscriptionEndDate
        };
        return ApiResponse<TenantProfileDto>.SuccessResponse(dto, "Profile retrieved successfully");
    }
}
