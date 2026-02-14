using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Notification : TenantEntity
{
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string Type { get; set; } = "Info"; // Info, Warning, Success, Error, Alert
    public bool IsRead { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
