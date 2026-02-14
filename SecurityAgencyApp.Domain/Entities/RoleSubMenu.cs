using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class RoleSubMenu : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid SubMenuId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual SubMenu SubMenu { get; set; } = null!;
}
