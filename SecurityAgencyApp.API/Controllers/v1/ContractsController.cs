using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Contracts.Commands.CreateContract;
using SecurityAgencyApp.Application.Features.Contracts.Commands.UpdateContract;
using SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractById;
using SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class ContractsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ContractsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<ContractListResponseDto>>> GetContracts(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? clientId = null,
        [FromQuery] string? status = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetContractListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            ClientId = clientId,
            Status = status,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SecurityAgencyApp.Application.Features.Contracts.Queries.GetContractById.ContractDto>>> GetContractById(Guid id)
    {
        var result = await _mediator.Send(new GetContractByIdQuery { Id = id });
        if (result.Success) return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateContract([FromBody] CreateContractCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetContractById), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateContract(Guid id, [FromBody] UpdateContractCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }
}
