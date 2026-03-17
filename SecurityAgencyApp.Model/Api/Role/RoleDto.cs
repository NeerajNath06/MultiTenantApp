namespace SecurityAgencyApp.Model.Api;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsSystemRole { get; set; }
    public bool IsActive { get; set; }
    public int UserCount { get; set; }
    public int PermissionCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<Guid>? PermissionIds { get; set; }
    public List<Guid>? MenuIds { get; set; }
    public List<Guid>? SubMenuIds { get; set; }
}
