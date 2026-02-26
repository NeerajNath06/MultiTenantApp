using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Sites.Commands.CreateSite;
using SecurityAgencyApp.Application.Features.Sites.Commands.DeleteSite;
using SecurityAgencyApp.Application.Features.Sites.Commands.UpdateSite;
using SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteById;
using SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteList;
using SecurityAgencyApp.Application.Features.Sites.Queries.GetSupervisorsBySite;
using GetSiteById = SecurityAgencyApp.Application.Features.Sites.Queries.GetSiteById;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class SitesController : ControllerBase
{
    private readonly IMediator _mediator;

    public SitesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all sites
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<SiteListResponseDto>>> GetSites(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? search = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "asc",
        [FromQuery] Guid? supervisorId = null)
    {
        var query = new GetSiteListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            IncludeInactive = includeInactive,
            Search = search,
            SortBy = sortBy,
            SortDirection = sortDirection,
            SupervisorId = supervisorId
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get supervisors assigned to this site (only those linked in Site Supervisors when site was created/updated).
    /// Use this when site is selected in Assign Guard form to populate supervisor dropdown.
    /// </summary>
    [HttpGet("{id}/Supervisors")]
    public async Task<ActionResult<ApiResponse<GetSupervisorsBySiteResponse>>> GetSupervisorsBySite(Guid id)
    {
        var query = new GetSupervisorsBySiteQuery { SiteId = id };
        var result = await _mediator.Send(query);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }

    /// <summary>
    /// Get site by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<GetSiteById.SiteDto>>> GetSiteById(Guid id)
    {
        var query = new GetSiteByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create a new site
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateSite([FromBody] CreateSiteCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetSiteById), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update site
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSite(Guid id, [FromBody] UpdateSiteCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Delete site
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSite(Guid id)
    {
        var command = new DeleteSiteCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
