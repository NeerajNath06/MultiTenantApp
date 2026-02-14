using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Clients.Commands.CreateClient;
using SecurityAgencyApp.Application.Features.Clients.Commands.UpdateClient;
using SecurityAgencyApp.Application.Features.Clients.Queries.GetClientById;
using SecurityAgencyApp.Application.Features.Clients.Queries.GetClientList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ClientsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ClientListResponseDto>>> GetClients(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? status = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var query = new GetClientListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            Status = status,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SecurityAgencyApp.Application.Features.Clients.Queries.GetClientById.ClientDto>>> GetClientById(Guid id)
    {
        var result = await _mediator.Send(new GetClientByIdQuery { Id = id });
        if (result.Success) return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateClient([FromBody] CreateClientCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetClientById), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateClient(Guid id, [FromBody] UpdateClientCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
