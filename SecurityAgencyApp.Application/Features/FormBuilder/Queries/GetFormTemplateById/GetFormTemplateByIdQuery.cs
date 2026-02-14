using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Queries.GetFormTemplateById;

public class GetFormTemplateByIdQuery : IRequest<ApiResponse<FormTemplateDto>>
{
    public Guid Id { get; set; }
}

public class FormTemplateDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; }
    public bool IsActive { get; set; }
    public List<FormFieldDto> Fields { get; set; } = new();
}

public class FormFieldDto
{
    public Guid Id { get; set; }
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
