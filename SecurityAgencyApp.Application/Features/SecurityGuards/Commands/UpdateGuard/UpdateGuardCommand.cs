using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.UpdateGuard;

public class UpdateGuardCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public Gender Gender { get; set; }
    public DateTime DateOfBirth { get; set; }
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? JoiningDate { get; set; }
    public bool IsActive { get; set; } = true;
    /// <summary>User (Supervisor) ID responsible for this guard.</summary>
    public Guid? SupervisorId { get; set; }
    /// <summary>Optional profile photo path. When provided, updates PhotoPath; when omitted, existing PhotoPath is preserved.</summary>
    public string? PhotoPath { get; set; }
}
