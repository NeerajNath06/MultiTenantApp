using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.Payments.Commands.CreatePayment;
using SecurityAgencyApp.Application.Features.Payments.Commands.DeletePayment;
using SecurityAgencyApp.Application.Features.Payments.Commands.UpdatePayment;
using SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentById;
using SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class PaymentsController : GenericCrudControllerBase<
    PaymentListResponseDto,
    SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentById.PaymentDto,
    GetPaymentListQuery,
    GetPaymentByIdQuery,
    CreatePaymentCommand,
    UpdatePaymentCommand,
    DeletePaymentCommand>
{
    public PaymentsController(IMediator mediator) : base(mediator) { }

    protected override GetPaymentByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdatePaymentCommand command, Guid id) => command.Id = id;

    protected override DeletePaymentCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
