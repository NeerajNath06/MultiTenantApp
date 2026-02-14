using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Equipment.Commands.CreateEquipment;
using SecurityAgencyApp.Application.Features.Equipment.Queries.GetEquipmentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class EquipmentController : ControllerBase
{
    private readonly IMediator _mediator;

    public EquipmentController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<EquipmentListResponseDto>>> GetEquipment(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] string? status = null,
        [FromQuery] Guid? assignedToGuardId = null,
        [FromQuery] Guid? assignedToSiteId = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc")
    {
        var query = new GetEquipmentListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            Category = category,
            Status = status,
            AssignedToGuardId = assignedToGuardId,
            AssignedToSiteId = assignedToSiteId,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateEquipment([FromBody] CreateEquipmentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetEquipment), new { id = result.Data }, result);
        return BadRequest(result);
    }
}
