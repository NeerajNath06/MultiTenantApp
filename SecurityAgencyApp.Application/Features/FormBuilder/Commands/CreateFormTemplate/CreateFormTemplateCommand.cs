using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Commands.CreateFormTemplate;

public class CreateFormTemplateCommand : IRequest<ApiResponse<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public bool IsSystemTemplate { get; set; } = false;
    public List<FormFieldDto> Fields { get; set; } = new();
}

public class FormFieldDto
{
    public string FieldName { get; set; } = string.Empty;
    public string FieldLabel { get; set; } = string.Empty;
    public string FieldType { get; set; } = string.Empty;
    public int FieldOrder { get; set; }
    public bool IsRequired { get; set; } = false;
    public string? DefaultValue { get; set; }
    public string? Placeholder { get; set; }
    public string? ValidationRules { get; set; }
    public string? Options { get; set; }
}
