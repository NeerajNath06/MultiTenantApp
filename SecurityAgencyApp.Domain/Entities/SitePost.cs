using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class SitePost : TenantEntity
{
    public Guid SiteId { get; set; }
    public Guid? BranchId { get; set; }
    public string PostCode { get; set; } = string.Empty;
    public string PostName { get; set; } = string.Empty;
    public string? ShiftName { get; set; }
    public int SanctionedStrength { get; set; }
    public string? GenderRequirement { get; set; }
    public string? SkillRequirement { get; set; }
    public bool RequiresWeapon { get; set; }
    public bool RelieverRequired { get; set; }
    public string? WeeklyOffPattern { get; set; }
    public bool IsActive { get; set; } = true;

    public virtual Site Site { get; set; } = null!;
    public virtual Branch? Branch { get; set; }
}
