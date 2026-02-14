using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Menu : TenantEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Icon { get; set; }
    public string? Route { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsSystemMenu { get; set; } = false;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual ICollection<SubMenu> SubMenus { get; set; } = new List<SubMenu>();
    public virtual ICollection<RoleMenu> RoleMenus { get; set; } = new List<RoleMenu>();
    public virtual ICollection<UserMenu> UserMenus { get; set; } = new List<UserMenu>();
    public virtual ICollection<MenuPermission> MenuPermissions { get; set; } = new List<MenuPermission>();
}
