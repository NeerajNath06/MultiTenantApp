namespace SecurityAgencyApp.Web.Models.Api;

public class FormTemplateListResponse
{
    public List<FormTemplateItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class FormTemplateItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; }
    public bool IsActive { get; set; }
    public int FieldCount { get; set; }
    public int SubmissionCount { get; set; }
    public DateTime CreatedDate { get; set; }
}

public class FormTemplateDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; }
    public bool IsActive { get; set; }
    public List<FormFieldItemDto> Fields { get; set; } = new();
}

public class CreateFormFieldRequest
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public int FieldOrder { get; set; }
    public bool IsRequired { get; set; }
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? ValidationRules { get; set; }
    public string? Options { get; set; }
}

public class FormFieldItemDto : CreateFormFieldRequest
{
    public Guid Id { get; set; }
}

public class CreateFormTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; }
    public List<CreateFormFieldRequest> Fields { get; set; } = new();
}

public class SubmitFormRequest
{
    public Guid FormTemplateId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string? Remarks { get; set; }
}
