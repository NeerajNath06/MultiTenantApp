using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class UserMenu : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid MenuId { get; set; }

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Menu Menu { get; set; } = null!;
}
