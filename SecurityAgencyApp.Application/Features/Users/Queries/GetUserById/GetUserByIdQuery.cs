using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQuery : IRequest<ApiResponse<UserDto>>
{
    public Guid Id { get; set; }
}

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
    public Guid? DepartmentId { get; set; }
    public string? DepartmentName { get; set; }
    public Guid? DesignationId { get; set; }
    public string? DesignationName { get; set; }
    public bool IsActive { get; set; }
    public List<string> Roles { get; set; } = new();
    public List<Guid> RoleIds { get; set; } = new();
    public DateTime CreatedDate { get; set; }
}
