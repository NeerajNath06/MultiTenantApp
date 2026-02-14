using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Domain.Entities;

public class IncidentReport : TenantEntity
{
    public string IncidentNumber { get; set; } = string.Empty;
    public Guid SiteId { get; set; }
    public Guid? GuardId { get; set; }
    public Guid ReportedBy { get; set; }
    public DateTime IncidentDate { get; set; }
    public string IncidentType { get; set; } = string.Empty;
    public IncidentSeverity Severity { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? ActionTaken { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Site Site { get; set; } = null!;
    public virtual SecurityGuard? Guard { get; set; }
    public virtual User ReportedByUser { get; set; } = null!;
    public virtual ICollection<IncidentAttachment> Attachments { get; set; } = new List<IncidentAttachment>();
}
