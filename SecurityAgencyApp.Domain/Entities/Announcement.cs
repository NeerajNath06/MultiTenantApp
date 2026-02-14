using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class Announcement : TenantEntity
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Category { get; set; } = "general"; // general, urgent, policy, event, training
    public Guid? PostedByUserId { get; set; }
    public string? PostedByName { get; set; }
    public DateTime PostedAt { get; set; }
    public bool IsPinned { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual User? PostedByUser { get; set; }
}
