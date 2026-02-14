using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class TrainingRecord : TenantEntity
{
    public Guid GuardId { get; set; }
    public string TrainingType { get; set; } = string.Empty; // Basic, Advanced, Fire Safety, First Aid, Weapon Training, etc.
    public string TrainingName { get; set; } = string.Empty;
    public string? TrainingProvider { get; set; }
    public DateTime TrainingDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public string Status { get; set; } = "Completed"; // Scheduled, Completed, Failed, Expired
    public string? CertificateNumber { get; set; }
    public string? CertificatePath { get; set; }
    public decimal? Score { get; set; }
    public string? Remarks { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual SecurityGuard Guard { get; set; } = null!;
}
