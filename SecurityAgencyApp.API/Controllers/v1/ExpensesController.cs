using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Expenses.Commands.CreateExpense;
using SecurityAgencyApp.Application.Features.Expenses.Queries.GetExpenseList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ExpensesController : ControllerBase
{
    private readonly IMediator _mediator;

    public ExpensesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ExpenseListResponseDto>>> GetExpenses(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetExpenseListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            Category = category,
            Status = status,
            SiteId = siteId,
            GuardId = guardId,
            StartDate = startDate,
            EndDate = endDate,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateExpense([FromBody] CreateExpenseCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetExpenses), new { id = result.Data }, result);
        return BadRequest(result);
    }
}
