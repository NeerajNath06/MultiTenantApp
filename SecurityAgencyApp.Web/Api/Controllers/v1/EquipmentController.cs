using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Equipment.Commands.CreateEquipment;
using SecurityAgencyApp.Application.Features.Equipment.Commands.DeleteEquipment;
using SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentById;
using SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class EquipmentController : GenericReadCreateDeleteControllerBase<
    EquipmentListResponseDto,
    EquipmentDetailDto,
    GetEquipmentListQuery,
    GetEquipmentByIdQuery,
    CreateEquipmentCommand,
    DeleteEquipmentCommand>
{
    public EquipmentController(IMediator mediator) : base(mediator) { }

    protected override GetEquipmentByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override DeleteEquipmentCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
