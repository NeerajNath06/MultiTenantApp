using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class RoleMenu : BaseEntity
{
    public Guid RoleId { get; set; }
    public Guid MenuId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
}
