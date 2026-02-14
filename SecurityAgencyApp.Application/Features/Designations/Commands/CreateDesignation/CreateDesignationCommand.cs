using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Designations.Commands.CreateDesignation;

public class CreateDesignationCommand : IRequest<ApiResponse<Guid>>
{
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public Guid? DepartmentId { get; set; }
    public string? Description { get; set; }
}
