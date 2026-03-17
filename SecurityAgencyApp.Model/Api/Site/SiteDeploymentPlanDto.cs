using System.ComponentModel.DataAnnotations;

namespace SecurityAgencyApp.Model.Api;

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
