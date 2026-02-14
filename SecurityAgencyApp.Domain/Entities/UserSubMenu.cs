using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class UserSubMenu : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid SubMenuId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual SubMenu SubMenu { get; set; } = null!;
}
