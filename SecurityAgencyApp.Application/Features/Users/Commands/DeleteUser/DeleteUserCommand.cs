using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Users.Commands.DeleteUser;

public class DeleteUserCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
