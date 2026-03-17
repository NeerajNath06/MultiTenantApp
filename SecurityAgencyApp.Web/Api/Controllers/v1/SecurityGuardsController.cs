using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.SecurityGuards.Commands.CreateGuard;
using SecurityAgencyApp.Application.Features.SecurityGuards.Commands.DeleteGuard;
using SecurityAgencyApp.Application.Features.SecurityGuards.Commands.UpdateGuard;
using SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardById;
using SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class SecurityGuardsController : GenericCrudControllerBase<
    GuardListResponseDto,
    SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardById.GuardDto,
    GetGuardListQuery,
    GetGuardByIdQuery,
    CreateGuardCommand,
    UpdateGuardCommand,
    DeleteGuardCommand>
{
    public SecurityGuardsController(IMediator mediator) : base(mediator) { }

    protected override GetGuardByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateGuardCommand command, Guid id) => command.Id = id;

    protected override DeleteGuardCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
