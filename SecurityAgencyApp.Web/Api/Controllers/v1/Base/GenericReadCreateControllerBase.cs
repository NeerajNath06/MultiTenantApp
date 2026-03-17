using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
public abstract class GenericReadCreateControllerBase<TListResponse, TDetailsResponse, TListQuery, TGetByIdQuery, TCreateCommand> : ControllerBase
    where TListQuery : class, IRequest<ApiResponse<TListResponse>>, new()
    where TGetByIdQuery : class, IRequest<ApiResponse<TDetailsResponse>>
    where TCreateCommand : class, IRequest<ApiResponse<Guid>>
{
    private readonly IMediator _mediator;

    protected GenericReadCreateControllerBase(IMediator mediator)
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
    public virtual async Task<ActionResult<ApiResponse<Guid>>> Create([FromBody] TCreateCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);

        return CreatedAtAction(nameof(GetById), new { id = result.Data }, result);
    }

    protected abstract TGetByIdQuery CreateGetByIdQuery(Guid id);
}
