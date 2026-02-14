using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.DeleteRole;

public class DeleteRoleCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
