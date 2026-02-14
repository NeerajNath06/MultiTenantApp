using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class FormTemplate : BaseEntity
{
    public Guid? TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; } = false;
    public bool IsActive { get; set; } = true;
    public Guid CreatedBy { get; set; }

    // Navigation properties
    public virtual Tenant? Tenant { get; set; }
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual ICollection<FormField> Fields { get; set; } = new List<FormField>();
    public virtual ICollection<FormSubmission> Submissions { get; set; } = new List<FormSubmission>();
}
