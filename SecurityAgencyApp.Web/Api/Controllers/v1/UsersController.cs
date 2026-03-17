using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Users.Commands.CreateUser;
using SecurityAgencyApp.Application.Features.Users.Commands.DeleteUser;
using SecurityAgencyApp.Application.Features.Users.Commands.UpdateUser;
using SecurityAgencyApp.Application.Features.Users.Queries.GetUserById;
using SecurityAgencyApp.Application.Features.Users.Queries.GetUserList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class UsersController : GenericCrudControllerBase<
    UserListResponseDto,
    SecurityAgencyApp.Application.Features.Users.Queries.GetUserById.UserDto,
    GetUserListQuery,
    GetUserByIdQuery,
    CreateUserCommand,
    UpdateUserCommand,
    DeleteUserCommand>
{
    public UsersController(IMediator mediator) : base(mediator) { }

    protected override GetUserByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateUserCommand command, Guid id) => command.Id = id;

    protected override DeleteUserCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
