using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Sites;

namespace SecurityAgencyApp.Application.Features.Sites.Commands.CreateSite;

public class CreateSiteCommand : IRequest<ApiResponse<Guid>>
{
    public string SiteCode { get; set; } = string.Empty;
    public string SiteName { get; set; } = string.Empty;
    public string ClientName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public string ContactPerson { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string? ContactEmail { get; set; }
    public bool IsActive { get; set; } = true;
    public double? Latitude { get; set; }
    public double? Longitude { get; set; }
    public int? GeofenceRadiusMeters { get; set; }
    public Guid? BranchId { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? MusterPoint { get; set; }
    public string? AccessZoneNotes { get; set; }
    public string? SiteInstructionBook { get; set; }
    public string? GeofenceExceptionNotes { get; set; }
    public List<SitePostInputDto>? Posts { get; set; }
    public SiteDeploymentPlanInputDto? DeploymentPlan { get; set; }
    /// <summary>User IDs (supervisors) to assign to this site.</summary>
    public List<Guid>? SupervisorIds { get; set; }
}
