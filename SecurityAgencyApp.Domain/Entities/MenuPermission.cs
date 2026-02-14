using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class MenuPermission : BaseEntity
{
    public Guid MenuId { get; set; }
    public Guid PermissionId { get; set; }

    // Navigation properties
    public virtual Menu Menu { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
