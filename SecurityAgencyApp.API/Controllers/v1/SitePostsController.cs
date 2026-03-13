using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SitePosts;
using SecurityAgencyApp.Application.Features.SitePosts.Commands.CreateSitePost;
using SecurityAgencyApp.Application.Features.SitePosts.Commands.DeleteSitePost;
using SecurityAgencyApp.Application.Features.SitePosts.Commands.UpdateSitePost;
using SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostById;
using SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class SitePostsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SitePostsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SitePostListResponseDto>>> GetSitePosts(
        [FromQuery] bool includeInactive = false,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] Guid? siteId = null,
        [FromQuery] Guid? branchId = null)
    {
        var result = await _mediator.Send(new GetSitePostListQuery
        {
            IncludeInactive = includeInactive,
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            SiteId = siteId,
            BranchId = branchId
        });
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<SitePostDto>>> GetSitePostById(Guid id)
    {
        var result = await _mediator.Send(new GetSitePostByIdQuery { Id = id });
        if (!result.Success)
            return NotFound(result);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateSitePost([FromBody] CreateSitePostCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return CreatedAtAction(nameof(GetSitePostById), new { id = result.Data }, result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSitePost(Guid id, [FromBody] UpdateSitePostCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSitePost(Guid id)
    {
        var result = await _mediator.Send(new DeleteSitePostCommand { Id = id });
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
