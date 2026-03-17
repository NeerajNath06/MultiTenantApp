using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.TenantProfile.Commands.UpdateTenantProfile;

public class UpdateTenantProfileCommandHandler : IRequestHandler<UpdateTenantProfileCommand, ApiResponse<bool>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public UpdateTenantProfileCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<bool>> Handle(UpdateTenantProfileCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<bool>.ErrorResponse("Tenant context not found");

        var tenant = await _unitOfWork.Repository<Tenant>().GetByIdAsync(_tenantContext.TenantId.Value, cancellationToken);
        if (tenant == null)
            return ApiResponse<bool>.ErrorResponse("Tenant not found");

        var tenantRepo = _unitOfWork.Repository<Tenant>();
        var existing = await tenantRepo.FirstOrDefaultAsync(
            t => t.Id != tenant.Id && (t.RegistrationNumber == request.RegistrationNumber || t.Email == request.Email),
            cancellationToken);
        if (existing != null)
            return ApiResponse<bool>.ErrorResponse("Another tenant with this registration number or email already exists");

        tenant.CompanyName = request.CompanyName.Trim();
        tenant.RegistrationNumber = request.RegistrationNumber.Trim();
        tenant.Email = request.Email.Trim();
        tenant.Phone = request.Phone.Trim();
        tenant.LegalName = Normalize(request.LegalName);
        tenant.TradeName = Normalize(request.TradeName);
        tenant.CompanyCode = Normalize(request.CompanyCode);
        tenant.CinNumber = Normalize(request.CinNumber);
        tenant.GstNumber = Normalize(request.GstNumber);
        tenant.PanNumber = Normalize(request.PanNumber);
        tenant.PfNumber = Normalize(request.PfNumber);
        tenant.EsicNumber = Normalize(request.EsicNumber);
        tenant.LabourLicenseNumber = Normalize(request.LabourLicenseNumber);
        tenant.OwnerName = Normalize(request.OwnerName);
        tenant.ComplianceOfficerName = Normalize(request.ComplianceOfficerName);
        tenant.BillingContactName = Normalize(request.BillingContactName);
        tenant.BillingContactPhone = Normalize(request.BillingContactPhone);
        tenant.BillingEmail = Normalize(request.BillingEmail);
        tenant.EscalationContactName = Normalize(request.EscalationContactName);
        tenant.EscalationContactPhone = Normalize(request.EscalationContactPhone);
        tenant.SupportEmail = Normalize(request.SupportEmail);
        tenant.Address = string.IsNullOrWhiteSpace(request.Address) ? null : request.Address.Trim();
        tenant.City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim();
        tenant.State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim();
        tenant.Country = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country.Trim();
        tenant.PinCode = string.IsNullOrWhiteSpace(request.PinCode) ? null : request.PinCode.Trim();
        tenant.Website = string.IsNullOrWhiteSpace(request.Website) ? null : request.Website.Trim();
        tenant.TaxId = string.IsNullOrWhiteSpace(request.TaxId) ? null : request.TaxId.Trim();
        tenant.TimeZone = string.IsNullOrWhiteSpace(request.TimeZone) ? tenant.TimeZone : request.TimeZone.Trim();
        tenant.Currency = string.IsNullOrWhiteSpace(request.Currency) ? tenant.Currency : request.Currency.Trim().ToUpperInvariant();
        tenant.InvoicePrefix = string.IsNullOrWhiteSpace(request.InvoicePrefix) ? tenant.InvoicePrefix : request.InvoicePrefix.Trim().ToUpperInvariant();
        tenant.PayrollPrefix = string.IsNullOrWhiteSpace(request.PayrollPrefix) ? tenant.PayrollPrefix : request.PayrollPrefix.Trim().ToUpperInvariant();
        tenant.SubscriptionPlan = string.IsNullOrWhiteSpace(request.SubscriptionPlan) ? tenant.SubscriptionPlan : request.SubscriptionPlan.Trim();
        tenant.SeatLimit = request.SeatLimit;
        tenant.BranchLimit = request.BranchLimit;
        tenant.StorageLimitGb = request.StorageLimitGb;
        tenant.OnboardingStatus = string.IsNullOrWhiteSpace(request.OnboardingStatus) ? tenant.OnboardingStatus : request.OnboardingStatus.Trim();
        tenant.ActivationStatus = string.IsNullOrWhiteSpace(request.ActivationStatus) ? tenant.ActivationStatus : request.ActivationStatus.Trim();
        tenant.IsKycVerified = request.IsKycVerified;
        tenant.OnboardingChecklistCompleted = request.OnboardingChecklistCompleted;
        tenant.ModifiedDate = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return ApiResponse<bool>.SuccessResponse(true);
    }

    private static string? Normalize(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
