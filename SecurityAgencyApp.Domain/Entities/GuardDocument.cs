using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class GuardDocument : BaseEntity
{
    public Guid GuardId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public bool IsVerified { get; set; } = false;
    public Guid? VerifiedBy { get; set; }
    public DateTime? VerifiedDate { get; set; }

    // Navigation properties
    public virtual SecurityGuard Guard { get; set; } = null!;
    public virtual User? VerifiedByUser { get; set; }
}

