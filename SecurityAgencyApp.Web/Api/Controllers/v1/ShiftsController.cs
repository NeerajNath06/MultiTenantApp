using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Shifts.Commands.CreateShift;
using SecurityAgencyApp.Application.Features.Shifts.Commands.DeleteShift;
using SecurityAgencyApp.Application.Features.Shifts.Commands.UpdateShift;
using SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftById;
using SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftList;
using GetShiftById = SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftById;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class ShiftsController : GenericCrudControllerBase<
    ShiftListResponseDto,
    GetShiftById.ShiftDto,
    GetShiftListQuery,
    GetShiftByIdQuery,
    CreateShiftCommand,
    UpdateShiftCommand,
    DeleteShiftCommand>
{
    public ShiftsController(IMediator mediator) : base(mediator) { }

    protected override GetShiftByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateShiftCommand command, Guid id) => command.Id = id;

    protected override DeleteShiftCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
