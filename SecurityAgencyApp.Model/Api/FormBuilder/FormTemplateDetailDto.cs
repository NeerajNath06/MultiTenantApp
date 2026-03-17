namespace SecurityAgencyApp.Model.Api;

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
