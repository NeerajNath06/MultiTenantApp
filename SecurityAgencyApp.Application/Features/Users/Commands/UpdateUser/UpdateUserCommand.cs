using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Users.Commands.UpdateUser;

public class UpdateUserCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AadharNumber { get; set; }
    public string? PANNumber { get; set; }
    public string? UAN { get; set; }
    public Guid? DepartmentId { get; set; }
    public Guid? DesignationId { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid>? RoleIds { get; set; }
    /// <summary>If set, updates password and stores plain copy in Password for recovery.</summary>
    public string? NewPassword { get; set; }
}
