using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class SitePostItemDto
{
    public Guid Id { get; set; }
    public Guid SiteId { get; set; }
    public string SiteName { get; set; } = string.Empty;
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string PostCode { get; set; } = string.Empty;
    public string PostName { get; set; } = string.Empty;
    public string? ShiftName { get; set; }
    public int SanctionedStrength { get; set; }
    public string? GenderRequirement { get; set; }
    public string? SkillRequirement { get; set; }
    public bool RequiresWeapon { get; set; }
    public bool RelieverRequired { get; set; }
    public string? WeeklyOffPattern { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
