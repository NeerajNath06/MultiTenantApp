using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Domain.Entities;

public class FormSubmission : TenantEntity
{
    public Guid FormTemplateId { get; set; }
    public Guid SubmittedBy { get; set; }
    public DateTime SubmissionDate { get; set; } = DateTime.UtcNow;
    public FormSubmissionStatus Status { get; set; } = FormSubmissionStatus.Submitted;
    public Guid? ApprovedById { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string? Remarks { get; set; }

    // Navigation properties
    public virtual Tenant Tenant { get; set; } = null!;
    public virtual FormTemplate FormTemplate { get; set; } = null!;
    public virtual User SubmittedByUser { get; set; } = null!;
    public virtual User? ApprovedByUser { get; set; }
    public virtual ICollection<FormSubmissionData> SubmissionData { get; set; } = new List<FormSubmissionData>();
}
