using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Authentication.Commands.Login;

public class LoginCommand : IRequest<ApiResponse<LoginResponseDto>>
{
    /// <summary>Login with username (Web).</summary>
    public string UserName { get; set; } = string.Empty;
    /// <summary>Login with email (Mobile sends userEmail). Used when UserName is empty.</summary>
    public string? UserEmail { get; set; }
    public string Password { get; set; } = string.Empty;
    public bool RememberMe { get; set; } = false;
}

public class LoginResponseDto
{
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public UserDto User { get; set; } = null!;
}

public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Permissions { get; set; } = new();
    /// <summary>Set when user has Security Guard or Supervisor role (linked guard record for mobile app).</summary>
    public Guid? GuardId { get; set; }
    /// <summary>True when user has Supervisor role – can see only assigned guards.</summary>
    public bool IsSupervisor { get; set; }
}
