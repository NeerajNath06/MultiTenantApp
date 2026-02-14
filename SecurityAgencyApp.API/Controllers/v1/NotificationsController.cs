using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Notifications.Commands.MarkAllAsRead;
using SecurityAgencyApp.Application.Features.Notifications.Commands.MarkAsRead;
using SecurityAgencyApp.Application.Features.Notifications.Commands.SendNotification;
using SecurityAgencyApp.Application.Features.Notifications.Queries.GetNotificationList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly IMediator _mediator;

    public NotificationsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<NotificationListResponseDto>>> GetNotifications(
        [FromQuery] Guid userId,
        [FromQuery] bool? isRead = null,
        [FromQuery] string? type = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetNotificationListQuery
        {
            UserId = userId,
            IsRead = isRead,
            Type = type,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPatch("{id}/read")]
    public async Task<ActionResult<ApiResponse<bool>>> MarkAsRead(Guid id, [FromQuery] Guid userId)
    {
        var command = new MarkAsReadCommand { Id = id, UserId = userId };
        var result = await _mediator.Send(command);
        if (result.Success) return Ok(result);
        return NotFound(result);
    }

    [HttpPatch("read-all")]
    public async Task<ActionResult<ApiResponse<int>>> MarkAllAsRead([FromQuery] Guid userId)
    {
        var command = new MarkAllAsReadCommand { UserId = userId };
        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>Send notification to one or more users (admin/supervisor sends to guards or others).</summary>
    [HttpPost("send")]
    public async Task<ActionResult<ApiResponse<SendNotificationResultDto>>> Send([FromBody] SendNotificationCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success && result.Data != null)
            return Ok(result);
        return BadRequest(result);
    }
}
