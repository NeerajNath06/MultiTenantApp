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
            return BadRequest(ApiResponse<bool>.ErrorResponse("Request body is required"));

        var command = new UpdateTenantProfileCommand
        {
            CompanyName = request.CompanyName ?? "",
            RegistrationNumber = request.RegistrationNumber ?? "",
            Email = request.Email ?? "",
            Phone = request.Phone ?? "",
            Address = request.Address,
            City = request.City,
            State = request.State,
            Country = request.Country,
            PinCode = request.PinCode,
            Website = request.Website,
            TaxId = request.TaxId
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
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PinCode { get; set; }
    public string? Website { get; set; }
    public string? TaxId { get; set; }
}
