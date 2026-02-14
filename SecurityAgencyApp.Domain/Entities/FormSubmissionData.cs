using SecurityAgencyApp.Domain.Common;

namespace SecurityAgencyApp.Domain.Entities;

public class FormSubmissionData : BaseEntity
{
    public Guid FormSubmissionId { get; set; }
    public Guid FormFieldId { get; set; }
    public string? FieldValue { get; set; } // Can store JSON for complex data

    // Navigation properties
    public virtual FormSubmission FormSubmission { get; set; } = null!;
    public virtual FormField FormField { get; set; } = null!;
}
