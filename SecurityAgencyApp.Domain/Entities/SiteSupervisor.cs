using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

/// <summary>
/// Links a User (supervisor) to a Site. When a supervisor is assigned to a site,
/// they see only these sites and guards deployed on these sites when logged in.
/// </summary>
public class SiteSupervisor : TenantEntity
{
    public Guid SiteId { get; set; }
    public Guid UserId { get; set; }

    public virtual Site Site { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
