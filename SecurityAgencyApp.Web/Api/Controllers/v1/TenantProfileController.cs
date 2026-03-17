using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Application.Features.TenantProfile.Commands.UpdateTenantProfile;
using SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;
using TenantProfileDto = SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile.TenantProfileDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TenantProfileController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ITenantContext _tenantContext;

    public TenantProfileController(IMediator mediator, ITenantContext tenantContext)
    {
        _mediator = mediator;
        _tenantContext = tenantContext;
    }

    /// <summary>Get current tenant's profile.</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<TenantProfileDto>>> GetProfile()
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest(ApiResponse<TenantProfileDto>.ErrorResponse("Tenant context not found"));
        var result = await _mediator.Send(new GetTenantProfileQuery());
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    /// <summary>Update current tenant's profile.</summary>
    [HttpPut]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateProfile([FromBody] UpdateTenantProfileRequest request)
    {
        if (!_tenantContext.TenantId.HasValue)
            return BadRequest(ApiResponse<bool>.ErrorResponse("Tenant context not found"));
        if (request == null)
            return BadRequest(ApiResponse<bool>.ErrorResponse(
                "Validation failed",
                new[] { ApiError.Create("Request body is required", "request") }));

        var command = new UpdateTenantProfileCommand
        {
            CompanyName = request.CompanyName ?? "",
            RegistrationNumber = request.RegistrationNumber ?? "",
            Email = request.Email ?? "",
            Phone = request.Phone ?? "",
            LegalName = request.LegalName,
            TradeName = request.TradeName,
            CompanyCode = request.CompanyCode,
            CinNumber = request.CinNumber,
            GstNumber = request.GstNumber,
            PanNumber = request.PanNumber,
            PfNumber = request.PfNumber,
            EsicNumber = request.EsicNumber,
            LabourLicenseNumber = request.LabourLicenseNumber,
            OwnerName = request.OwnerName,
            ComplianceOfficerName = request.ComplianceOfficerName,
            BillingContactName = request.BillingContactName,
            BillingContactPhone = request.BillingContactPhone,
            BillingEmail = request.BillingEmail,
            EscalationContactName = request.EscalationContactName,
            EscalationContactPhone = request.EscalationContactPhone,
            SupportEmail = request.SupportEmail,
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PinCode = request.PinCode,
            Website = request.Website,
            TaxId = request.TaxId,
            TimeZone = request.TimeZone,
            Currency = request.Currency,
            InvoicePrefix = request.InvoicePrefix,
            PayrollPrefix = request.PayrollPrefix,
            SubscriptionPlan = request.SubscriptionPlan,
            SeatLimit = request.SeatLimit,
            BranchLimit = request.BranchLimit,
            StorageLimitGb = request.StorageLimitGb,
            OnboardingStatus = request.OnboardingStatus,
            ActivationStatus = request.ActivationStatus,
            IsKycVerified = request.IsKycVerified,
            OnboardingChecklistCompleted = request.OnboardingChecklistCompleted
        };
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}

public class UpdateTenantProfileRequest
{
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? LegalName { get; set; }
    public string? TradeName { get; set; }
    public string? CompanyCode { get; set; }
    public string? CinNumber { get; set; }
    public string? GstNumber { get; set; }
    public string? PanNumber { get; set; }
    public string? PfNumber { get; set; }
    public string? EsicNumber { get; set; }
    public string? LabourLicenseNumber { get; set; }
    public string? OwnerName { get; set; }
    public string? ComplianceOfficerName { get; set; }
    public string? BillingContactName { get; set; }
    public string? BillingContactPhone { get; set; }
    public string? BillingEmail { get; set; }
    public string? EscalationContactName { get; set; }
    public string? EscalationContactPhone { get; set; }
    public string? SupportEmail { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PinCode { get; set; }
    public string? Website { get; set; }
    public string? TaxId { get; set; }
    public string? TimeZone { get; set; }
    public string? Currency { get; set; }
    public string? InvoicePrefix { get; set; }
    public string? PayrollPrefix { get; set; }
    public string? SubscriptionPlan { get; set; }
    public int? SeatLimit { get; set; }
    public int? BranchLimit { get; set; }
    public decimal? StorageLimitGb { get; set; }
    public string? OnboardingStatus { get; set; }
    public string? ActivationStatus { get; set; }
    public bool IsKycVerified { get; set; }
    public bool OnboardingChecklistCompleted { get; set; }
}
