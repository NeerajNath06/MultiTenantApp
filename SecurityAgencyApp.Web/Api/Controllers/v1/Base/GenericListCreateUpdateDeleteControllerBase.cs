using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
public abstract class GenericListCreateUpdateDeleteControllerBase<TListResponse, TListQuery, TCreateCommand, TUpdateCommand, TDeleteCommand> : ControllerBase
    where TListQuery : class, IRequest<ApiResponse<TListResponse>>, new()
    where TCreateCommand : class, IRequest<ApiResponse<Guid>>
    where TUpdateCommand : class, IRequest<ApiResponse<bool>>
    where TDeleteCommand : class, IRequest<ApiResponse<bool>>
{
    private readonly IMediator _mediator;

    protected GenericListCreateUpdateDeleteControllerBase(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public virtual async Task<ActionResult<ApiResponse<TListResponse>>> GetList([FromQuery] TListQuery query)
    {
        query ??= new TListQuery();
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public virtual async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] TCreateCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);

        return Ok(result);
    }

    [HttpPut("{id}")]
    public virtual async Task<ActionResult<ApiResponse<bool>>> Update(Guid id, [FromBody] TUpdateCommand command)
    {
        SetUpdateCommandId(command, id);
        var result = await _mediator.Send(command);
        if (result.Success)
            return Ok(result);

        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    public virtual async Task<ActionResult<ApiResponse<bool>>> Delete(Guid id)
    {
        var result = await _mediator.Send(CreateDeleteCommand(id));
        if (result.Success)
            return Ok(result);

        if (result.Message?.Contains("not found", StringComparison.OrdinalIgnoreCase) == true)
            return NotFound(result);

        return BadRequest(result);
    }

    protected abstract void SetUpdateCommandId(TUpdateCommand command, Guid id);
    protected abstract TDeleteCommand CreateDeleteCommand(Guid id);
}
