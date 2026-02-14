namespace SecurityAgencyApp.Web.Models.Api;

public class SubMenuListResponse
{
    public List<SubMenuItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class SubMenuItemDto
{
    public Guid Id { get; set; }
    public Guid MenuId { get; set; }
    public string MenuName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSubMenuRequest
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

public class UpdateSubMenuRequest : CreateSubMenuRequest
{
    public Guid Id { get; set; }
}
