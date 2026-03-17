namespace SecurityAgencyApp.Model.Api;

public class LoginUserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public Guid TenantId { get; set; }
    public string TenantName { get; set; } = string.Empty;
    public List<string>? Roles { get; set; }
    public List<string>? Permissions { get; set; }
    public Guid? GuardId { get; set; }
    public bool IsSupervisor { get; set; }
}
