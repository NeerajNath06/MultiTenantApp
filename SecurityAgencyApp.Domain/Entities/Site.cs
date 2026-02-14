using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Site : TenantEntity
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

    /// <summary>Site center latitude for geofencing (check-in/check-out).</summary>
    public double? Latitude { get; set; }
    /// <summary>Site center longitude for geofencing (check-in/check-out).</summary>
    public double? Longitude { get; set; }
    /// <summary>Allowed radius in meters; check-in/check-out only allowed within this distance.</summary>
    public int? GeofenceRadiusMeters { get; set; }

    public Guid? ClientId { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual Client? Client { get; set; }
    public virtual ICollection<GuardAssignment> GuardAssignments { get; set; } = new List<GuardAssignment>();
    public virtual ICollection<IncidentReport> IncidentReports { get; set; } = new List<IncidentReport>();
    public virtual ICollection<ContractSite> ContractSites { get; set; } = new List<ContractSite>();
    public virtual ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public virtual ICollection<Equipment> AssignedEquipment { get; set; } = new List<Equipment>();
    /// <summary>Supervisors assigned to this site (see only this site and its guards when logged in).</summary>
    public virtual ICollection<SiteSupervisor> SiteSupervisors { get; set; } = new List<SiteSupervisor>();
}
