using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Announcements.Commands.CreateAnnouncement;
using SecurityAgencyApp.Application.Features.Announcements.Commands.DeleteAnnouncement;
using SecurityAgencyApp.Application.Features.Announcements.Commands.UpdateAnnouncement;
using SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementById;
using SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementList;
using AnnouncementByIdDto = SecurityAgencyApp.Application.Features.Announcements.Queries.GetAnnouncementById.AnnouncementDto;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class AnnouncementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnnouncementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<AnnouncementListResponseDto>>> GetAnnouncements(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50,
        [FromQuery] string? search = null,
        [FromQuery] string? category = null,
        [FromQuery] bool? isPinned = null,
        [FromQuery] bool includeInactive = false,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortDirection = "desc")
    {
        var query = new GetAnnouncementListQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            Search = search,
            Category = category,
            IsPinned = isPinned,
            IncludeInactive = includeInactive,
            SortBy = sortBy,
            SortDirection = sortDirection
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<AnnouncementByIdDto>>> GetAnnouncementById(Guid id)
    {
        var result = await _mediator.Send(new GetAnnouncementByIdQuery { Id = id });
        if (result.Success && result.Data != null) return Ok(result);
        return NotFound(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateAnnouncement([FromBody] CreateAnnouncementCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success && result.Data != default)
            return CreatedAtAction(nameof(GetAnnouncementById), new { id = result.Data }, result);
        return BadRequest(result);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateAnnouncement(Guid id, [FromBody] UpdateAnnouncementCommand command)
    {
        command.Id = id;
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return BadRequest(result);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteAnnouncement(Guid id)
    {
        var result = await _mediator.Send(new DeleteAnnouncementCommand { Id = id });
        if (result.Success) return Ok(result);
        return NotFound(result);
    }
}
