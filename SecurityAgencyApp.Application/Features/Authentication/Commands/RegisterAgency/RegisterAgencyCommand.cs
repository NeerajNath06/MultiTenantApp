using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Authentication.Commands.RegisterAgency;

public class RegisterAgencyCommand : IRequest<ApiResponse<RegisterAgencyResponseDto>>
{
    // Agency/Tenant Information
    public string CompanyName { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Country { get; set; }
    public string? PinCode { get; set; }

    // Admin User Information
    public string AdminUserName { get; set; } = string.Empty;
    public string AdminEmail { get; set; } = string.Empty;
    public string AdminPassword { get; set; } = string.Empty;
    public string AdminFirstName { get; set; } = string.Empty;
    public string AdminLastName { get; set; } = string.Empty;
    public string? AdminPhoneNumber { get; set; }
}

public class RegisterAgencyResponseDto
{
    public Guid TenantId { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public Guid AdminUserId { get; set; }
    public string AdminUserName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
