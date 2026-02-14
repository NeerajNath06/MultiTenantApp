namespace SecurityAgencyApp.Web.Models.Api;

public class RoleListResponse
{
    public List<RoleDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

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

public class CreateRoleRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class UpdateRoleRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public List<Guid>? PermissionIds { get; set; }
}

public class PermissionListResponse
{
    public List<PermissionDto> Items { get; set; } = new();
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
}
