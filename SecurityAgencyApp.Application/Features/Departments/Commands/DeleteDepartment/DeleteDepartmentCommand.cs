using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Departments.Commands.DeleteDepartment;

public class DeleteDepartmentCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
