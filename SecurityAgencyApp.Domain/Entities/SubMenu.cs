using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class SubMenu : TenantEntity
{
    public Guid MenuId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
    public virtual ICollection<RoleSubMenu> RoleSubMenus { get; set; } = new List<RoleSubMenu>();
    public virtual ICollection<UserSubMenu> UserSubMenus { get; set; } = new List<UserSubMenu>();
    public virtual ICollection<SubMenuPermission> SubMenuPermissions { get; set; } = new List<SubMenuPermission>();
}
