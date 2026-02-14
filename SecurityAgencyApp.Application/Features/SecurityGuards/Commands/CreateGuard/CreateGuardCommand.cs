using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.CreateGuard;

public class CreateGuardCommand : IRequest<ApiResponse<Guid>>
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? AlternatePhone { get; set; }
    public DateTime DateOfBirth { get; set; }
    public Gender Gender { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string EmergencyContactName { get; set; } = string.Empty;
    public string EmergencyContactPhone { get; set; } = string.Empty;
    public DateTime JoiningDate { get; set; }
    /// <summary>When true, create a User account so this guard can login to the mobile app.</summary>
    public bool CreateLoginAccount { get; set; }
    /// <summary>Login username (required if CreateLoginAccount).</summary>
    public string? LoginUserName { get; set; }
    /// <summary>Login password (required if CreateLoginAccount, min 6 chars).</summary>
    public string? LoginPassword { get; set; }
    /// <summary>User (Supervisor) ID responsible for this guard.</summary>
    public Guid? SupervisorId { get; set; }
}
