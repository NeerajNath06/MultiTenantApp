using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
public abstract class GenericDetailedCrudControllerBase<TListResponse, TDetailsResponse, TListQuery, TGetByIdQuery, TCreateCommand, TCreateResponse, TUpdateCommand, TDeleteCommand> : ControllerBase
    where TListQuery : class, IRequest<ApiResponse<TListResponse>>, new()
    where TGetByIdQuery : class, IRequest<ApiResponse<TDetailsResponse>>
    where TCreateCommand : class, IRequest<ApiResponse<TCreateResponse>>
    where TUpdateCommand : class, IRequest<ApiResponse<bool>>
    where TDeleteCommand : class, IRequest<ApiResponse<bool>>
{
    private readonly IMediator _mediator;

    protected GenericDetailedCrudControllerBase(IMediator mediator)
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

    [HttpGet("{id}")]
    public virtual async Task<ActionResult<ApiResponse<TDetailsResponse>>> GetById(Guid id)
    {
        var result = await _mediator.Send(CreateGetByIdQuery(id));
        if (result.Success)
            return Ok(result);

        return NotFound(result);
    }

    [HttpPost]
    public virtual async Task<ActionResult<ApiResponse<TCreateResponse>>> Create([FromBody] TCreateCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success || result.Data == null)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), GetCreatedRouteValues(result.Data), result);
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

    protected abstract TGetByIdQuery CreateGetByIdQuery(Guid id);
    protected abstract void SetUpdateCommandId(TUpdateCommand command, Guid id);
    protected abstract TDeleteCommand CreateDeleteCommand(Guid id);
    protected abstract object GetCreatedRouteValues(TCreateResponse response);
}
