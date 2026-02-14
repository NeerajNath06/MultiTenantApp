using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class UserPermission : BaseEntity
{
    public Guid UserId { get; set; }
    public Guid PermissionId { get; set; }
    public bool IsGranted { get; set; } = true;

    // Navigation properties
    public virtual User User { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
