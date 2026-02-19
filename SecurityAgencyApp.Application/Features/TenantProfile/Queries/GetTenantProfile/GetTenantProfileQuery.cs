using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.TenantProfile.Queries.GetTenantProfile;

public class GetTenantProfileQuery : IRequest<ApiResponse<TenantProfileDto>>
{
}

public class TenantProfileDto
{
    public Guid Id { get; set; }
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
    public string? LogoPath { get; set; }
    public bool IsActive { get; set; }
    public DateTime SubscriptionStartDate { get; set; }
    public DateTime? SubscriptionEndDate { get; set; }
}
