using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Designations.Commands.CreateDesignation;
using SecurityAgencyApp.Application.Features.Designations.Commands.DeleteDesignation;
using SecurityAgencyApp.Application.Features.Designations.Commands.UpdateDesignation;
using SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationById;
using SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationList;
using GetDesignationById = SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationById;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class DesignationsController : GenericCrudControllerBase<
    DesignationListResponseDto,
    GetDesignationById.DesignationDto,
    GetDesignationListQuery,
    GetDesignationByIdQuery,
    CreateDesignationCommand,
    UpdateDesignationCommand,
    DeleteDesignationCommand>
{
    public DesignationsController(IMediator mediator) : base(mediator) { }

    protected override GetDesignationByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateDesignationCommand command, Guid id) => command.Id = id;

    protected override DeleteDesignationCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
