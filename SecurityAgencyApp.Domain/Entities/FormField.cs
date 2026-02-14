using SecurityAgencyApp.Domain.Common;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Domain.Entities;

public class FormField : BaseEntity
{
    public Guid FormTemplateId { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public FormFieldType FieldType { get; set; }
    public int FieldOrder { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? ValidationRules { get; set; } // JSON
    public string? Options { get; set; } // JSON for dropdown/radio options
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public virtual FormTemplate FormTemplate { get; set; } = null!;
    public virtual ICollection<FormSubmissionData> SubmissionData { get; set; } = new List<FormSubmissionData>();
}
