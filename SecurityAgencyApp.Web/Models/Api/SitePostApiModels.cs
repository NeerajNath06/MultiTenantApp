using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Web.Models.Api;

public class SitePostListResponse
{
    public List<SitePostItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? Search { get; set; }
    public Guid? SiteId { get; set; }
    public Guid? BranchId { get; set; }
}

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

public class CreateSitePostRequest
{
    [Required]
    [Display(Name = "Site")]
    public Guid SiteId { get; set; }

    [Display(Name = "Branch")]
    public Guid? BranchId { get; set; }

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
    public int SanctionedStrength { get; set; } = 1;

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

    [Display(Name = "Active")]
    public bool IsActive { get; set; } = true;
}

public class UpdateSitePostRequest : CreateSitePostRequest
{
    public Guid Id { get; set; }
}
