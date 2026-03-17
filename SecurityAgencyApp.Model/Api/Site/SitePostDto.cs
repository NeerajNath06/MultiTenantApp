using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

public class SitePostDto
{
    public Guid Id { get; set; }

    [Required]
    [Display(Name = "Post Code")]
    public string PostCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Post Name")]
    public string PostName { get; set; } = string.Empty;

    [Display(Name = "Shift Name")]
    public string? ShiftName { get; set; }

    [Range(0, 9999)]
    [Display(Name = "Sanctioned Strength")]
    public int SanctionedStrength { get; set; }

    [Display(Name = "Gender Requirement")]
    public string? GenderRequirement { get; set; }

    [Display(Name = "Skill Requirement")]
    public string? SkillRequirement { get; set; }

    [Display(Name = "Requires Weapon")]
    public bool RequiresWeapon { get; set; }

    [Display(Name = "Reliever Required")]
    public bool RelieverRequired { get; set; }

    [Display(Name = "Weekly Off Pattern")]
    public string? WeeklyOffPattern { get; set; }
    public bool IsActive { get; set; }
}
