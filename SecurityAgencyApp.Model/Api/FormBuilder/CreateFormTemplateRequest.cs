namespace SecurityAgencyApp.Model.Api;

public class CreateFormTemplateRequest
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; }
    public List<CreateFormFieldRequest> Fields { get; set; } = new();
}
