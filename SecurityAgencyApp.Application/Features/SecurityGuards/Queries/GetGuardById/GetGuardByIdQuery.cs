using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardById;

public class GetGuardByIdQuery : IRequest<ApiResponse<GuardDto>>
{
    public Guid Id { get; set; }
}

public class GuardDto
{
    public Guid Id { get; set; }
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PinCode { get; set; }
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public DateTime? JoiningDate { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
    /// <summary>Relative path or URL for guard profile photo.</summary>
    public string? PhotoPath { get; set; }
}
