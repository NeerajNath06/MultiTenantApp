using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

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
