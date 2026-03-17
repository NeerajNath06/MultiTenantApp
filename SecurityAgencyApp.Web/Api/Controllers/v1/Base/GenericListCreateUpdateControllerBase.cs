using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
public abstract class GenericListCreateUpdateControllerBase<TListResponse, TListQuery, TCreateCommand, TUpdateCommand> : ControllerBase
    where TListQuery : class, IRequest<ApiResponse<TListResponse>>, new()
    where TCreateCommand : class, IRequest<ApiResponse<Guid>>
    where TUpdateCommand : class, IRequest<ApiResponse<bool>>
{
    private readonly IMediator _mediator;

    protected GenericListCreateUpdateControllerBase(IMediator mediator)
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
        if (result.Success)
            return Ok(result);

        return BadRequest(result);
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

    protected abstract void SetUpdateCommandId(TUpdateCommand command, Guid id);
}
