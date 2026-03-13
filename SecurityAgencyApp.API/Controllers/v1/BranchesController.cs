using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Branches;
using SecurityAgencyApp.Application.Features.Branches.Commands.CreateBranch;
using SecurityAgencyApp.Application.Features.Branches.Commands.DeleteBranch;
using SecurityAgencyApp.Application.Features.Branches.Commands.UpdateBranch;
using SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchById;
using SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class BranchesController : ControllerBase
{
    private readonly IMediator _mediator;

    public BranchesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<BranchListResponseDto>>> GetBranches([FromQuery] bool includeInactive = false, [FromQuery] string? search = null)
    {
        var result = await _mediator.Send(new GetBranchListQuery
        {
            IncludeInactive = includeInactive,
            Search = search
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<BranchDto>>> GetBranchById(Guid id)
    {
        var result = await _mediator.Send(new GetBranchByIdQuery { Id = id });
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateBranch([FromBody] CreateBranchCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetBranchById), new { id = result.Data }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateBranch(Guid id, [FromBody] UpdateBranchCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteBranch(Guid id)
    {
        var result = await _mediator.Send(new DeleteBranchCommand { Id = id });
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
