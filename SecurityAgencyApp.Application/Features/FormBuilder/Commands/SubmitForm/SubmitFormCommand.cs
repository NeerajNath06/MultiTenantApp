using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.FormBuilder.Commands.SubmitForm;

public class SubmitFormCommand : IRequest<ApiResponse<Guid>>
{
    public Guid FormTemplateId { get; set; }
    public Dictionary<string, object> Data { get; set; } = new();
    public string? Remarks { get; set; }
}
