using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Web.Models.Api;

public class SiteListResponse
{
    public List<SiteDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class SiteDto
{
    public Guid Id { get; set; }
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public Guid? ClientId { get; set; }
    public Guid? BranchId { get; set; }
    public string? BranchName { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string? ContactPerson { get; set; }
    public string? ContactPhone { get; set; }
    public string? ContactEmail { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MusterPoint { get; set; }
    public string? AccessZoneNotes { get; set; }
    public string? SiteInstructionBook { get; set; }
    public string? GeofenceExceptionNotes { get; set; }
    public bool IsActive { get; set; }
    public int GuardCount { get; set; }
    public int PostsCount { get; set; }
    public bool HasActiveDeploymentPlan { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? GeofenceRadiusMeters { get; set; }
    public List<Guid> SupervisorIds { get; set; } = new();
    public List<SitePostDto> Posts { get; set; } = new();
    public SiteDeploymentPlanDto? ActiveDeploymentPlan { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}

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

public class SiteDeploymentPlanDto
{
    public Guid Id { get; set; }

    [Display(Name = "Effective From")]
    public DateTime EffectiveFrom { get; set; }

    [Display(Name = "Effective To")]
    public DateTime? EffectiveTo { get; set; }

    [Display(Name = "Reserve Pool Mapping")]
    public string? ReservePoolMapping { get; set; }

    [Display(Name = "Access Zones")]
    public string? AccessZones { get; set; }

    [Display(Name = "Emergency Contact Set")]
    public string? EmergencyContactSet { get; set; }

    [Display(Name = "Instruction Summary")]
    public string? InstructionSummary { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; }
}

public class CreateSiteRequest
{
    [Required]
    [Display(Name = "Site Code")]
    public string SiteCode { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Site Name")]
    public string SiteName { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Client Name")]
    public string ClientName { get; set; } = string.Empty;

    [Required]
    public string Address { get; set; } = string.Empty;

    [Required]
    public string City { get; set; } = string.Empty;

    [Required]
    public string State { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Pin Code")]
    public string PinCode { get; set; } = string.Empty;

    [Display(Name = "Contact Person")]
    public string? ContactPerson { get; set; }

    [Phone]
    [Display(Name = "Contact Phone")]
    public string? ContactPhone { get; set; }

    [EmailAddress]
    [Display(Name = "Contact Email")]
    public string? ContactEmail { get; set; }
    public Guid? BranchId { get; set; }

    [Display(Name = "Emergency Contact Name")]
    public string? EmergencyContactName { get; set; }

    [Phone]
    [Display(Name = "Emergency Contact Phone")]
    public string? EmergencyContactPhone { get; set; }

    [Display(Name = "Muster Point")]
    public string? MusterPoint { get; set; }

    [Display(Name = "Access Zone Notes")]
    public string? AccessZoneNotes { get; set; }

    [Display(Name = "Site Instruction Book")]
    public string? SiteInstructionBook { get; set; }

    [Display(Name = "Geofence Exception Notes")]
    public string? GeofenceExceptionNotes { get; set; }
    public bool IsActive { get; set; }
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }

    [Range(0, 100000)]
    [Display(Name = "Geofence Radius Meters")]
    public int? GeofenceRadiusMeters { get; set; }
    public List<Guid> SupervisorIds { get; set; } = new();
    public List<SitePostDto> Posts { get; set; } = new();
    public SiteDeploymentPlanDto? ActiveDeploymentPlan { get; set; }
}

public class UpdateSiteRequest : CreateSiteRequest
{
    public Guid Id { get; set; }
    public Guid? ClientId { get; set; }
}
