using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
public abstract class GenericListCreateControllerBase<TListResponse, TListQuery, TCreateCommand, TCreateResponse> : ControllerBase
    where TListQuery : class, IRequest<ApiResponse<TListResponse>>, new()
    where TCreateCommand : class, IRequest<ApiResponse<TCreateResponse>>
{
    private readonly IMediator _mediator;

    protected GenericListCreateControllerBase(IMediator mediator)
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
    public virtual async Task<ActionResult<ApiResponse<TCreateResponse>>> Create([FromBody] TCreateCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);

        return BuildCreateSuccess(command, result);
    }

    protected abstract ActionResult<ApiResponse<TCreateResponse>> BuildCreateSuccess(TCreateCommand command, ApiResponse<TCreateResponse> result);
}
