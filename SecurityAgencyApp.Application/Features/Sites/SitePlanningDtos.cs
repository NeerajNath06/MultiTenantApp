namespace SecurityAgencyApp.Application.Features.Sites;

public class SitePostInputDto
{
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
}

public class SiteDeploymentPlanInputDto
{
    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string? ReservePoolMapping { get; set; }
    public string? AccessZones { get; set; }
    public string? EmergencyContactSet { get; set; }
    public string? InstructionSummary { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
}

public class SitePostDto : SitePostInputDto
{
    public Guid Id { get; set; }
}

public class SiteDeploymentPlanDto : SiteDeploymentPlanInputDto
{
    public Guid Id { get; set; }
}
