using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Payments.Commands.CreatePayment;
using SecurityAgencyApp.Application.Features.Payments.Commands.UpdatePayment;
using SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentById;
using SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class PaymentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaymentListResponseDto>>> GetPayments(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? billId = null,
        [FromQuery] Guid? clientId = null,
        [FromQuery] string? status = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetPaymentListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            BillId = billId,
            ClientId = clientId,
            Status = status,
            StartDate = startDate,
            EndDate = endDate,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SecurityAgencyApp.Application.Features.Payments.Queries.GetPaymentById.PaymentDto>>> GetPaymentById(Guid id)
    {
        var result = await _mediator.Send(new GetPaymentByIdQuery { Id = id });
        if (result.Success) return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreatePayment([FromBody] CreatePaymentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetPaymentById), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdatePayment(Guid id, [FromBody] UpdatePaymentCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
