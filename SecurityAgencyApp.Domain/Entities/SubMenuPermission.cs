using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class SubMenuPermission : BaseEntity
{
    public Guid SubMenuId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public virtual SubMenu SubMenu { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
