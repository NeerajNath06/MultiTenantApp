using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Queries.GetFormTemplateList;

public class GetFormTemplateListQuery : IRequest<ApiResponse<FormTemplateListResponseDto>>
{
    public bool IncludeInactive { get; set; } = false;
    public string? Category { get; set; }
}

public class FormTemplateListResponseDto
{
    public List<FormTemplateDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
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
    public int FieldCount { get; set; }
    public int SubmissionCount { get; set; }
    public DateTime CreatedDate { get; set; }
}
