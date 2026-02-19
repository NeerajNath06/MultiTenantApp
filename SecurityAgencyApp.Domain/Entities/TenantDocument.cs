using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class TenantDocument : BaseEntity
{
    public Guid TenantId { get; set; }
    public string DocumentType { get; set; } = string.Empty;
    public string? DocumentNumber { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public string? OriginalFileName { get; set; }
    public DateTime? ExpiryDate { get; set; }

    public virtual Tenant Tenant { get; set; } = null!;
}
