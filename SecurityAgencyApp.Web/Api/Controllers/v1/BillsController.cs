using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Bills.Commands.CreateBill;
using SecurityAgencyApp.Application.Features.Bills.Commands.DeleteBill;
using SecurityAgencyApp.Application.Features.Bills.Commands.UpdateBill;
using SecurityAgencyApp.Application.Features.Bills.Queries.GetBillById;
using SecurityAgencyApp.Application.Features.Bills.Queries.GetBillList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class BillsController : GenericCrudControllerBase<
    BillListResponseDto,
    SecurityAgencyApp.Application.Features.Bills.Queries.GetBillById.BillDto,
    GetBillListQuery,
    GetBillByIdQuery,
    CreateBillCommand,
    UpdateBillCommand,
    DeleteBillCommand>
{
    public BillsController(IMediator mediator) : base(mediator) { }

    protected override GetBillByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateBillCommand command, Guid id) => command.Id = id;

    protected override DeleteBillCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
