using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Roles.Commands.AssignPermissionsToRole;
using SecurityAgencyApp.Application.Features.Roles.Commands.CreateRole;
using SecurityAgencyApp.Application.Features.Roles.Commands.DeleteRole;
using SecurityAgencyApp.Application.Features.Roles.Commands.UpdateRole;
using SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleById;
using SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleList;
using GetRoleByIdDto = SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleById.RoleDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class RolesController : GenericCrudControllerBase<
    RoleListResponseDto,
    GetRoleByIdDto,
    GetRoleListQuery,
    GetRoleByIdQuery,
    CreateRoleCommand,
    UpdateRoleCommand,
    DeleteRoleCommand>
{
    private readonly IMediator _mediator;

    public RolesController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    protected override GetRoleByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateRoleCommand command, Guid id) => command.Id = id;

    protected override DeleteRoleCommand CreateDeleteCommand(Guid id) => new() { Id = id };

    [HttpPost("{roleId}/assign-permissions")]
    public async Task<ActionResult<SecurityAgencyApp.Application.Common.Models.ApiResponse<bool>>> AssignPermissionsToRole(Guid roleId, [FromBody] List<Guid> permissionIds)
    {
        var command = new AssignPermissionsToRoleCommand { RoleId = roleId, PermissionIds = permissionIds ?? new List<Guid>() };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
