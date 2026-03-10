using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.TrainingRecords.Commands.CreateTrainingRecord;
using SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordById;
using SecurityAgencyApp.Application.Features.TrainingRecords.Commands.DeleteTrainingRecord;
using SecurityAgencyApp.Application.Features.TrainingRecords.Queries.GetTrainingRecordList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class TrainingRecordsController : ControllerBase
{
    private readonly IMediator _mediator;

    public TrainingRecordsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<TrainingRecordListResponseDto>>> GetTrainingRecords(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? guardId = null,
        [FromQuery] string? trainingType = null,
        [FromQuery] string? status = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetTrainingRecordListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            GuardId = guardId,
            TrainingType = trainingType,
            Status = status,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TrainingRecordDetailDto>>> GetTrainingRecordById(Guid id)
    {
        var result = await _mediator.Send(new GetTrainingRecordByIdQuery { Id = id });
        if (!result.Success) return NotFound(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteTrainingRecord(Guid id)
    {
        var result = await _mediator.Send(new DeleteTrainingRecordCommand { Id = id });
        if (result.Success) return Ok(result);
        if (result.Message?.Contains("not found") == true) return NotFound(result);
        return BadRequest(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateTrainingRecord([FromBody] CreateTrainingRecordCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
            return CreatedAtAction(nameof(GetTrainingRecords), new { id = result.Data }, result);
        return BadRequest(result);
    }
}
