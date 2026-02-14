using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SubMenus.Commands.CreateSubMenu;
using SecurityAgencyApp.Application.Features.SubMenus.Commands.DeleteSubMenu;
using SecurityAgencyApp.Application.Features.SubMenus.Commands.UpdateSubMenu;
using SecurityAgencyApp.Application.Features.SubMenus.Queries.GetSubMenuList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class SubMenusController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubMenusController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<SubMenuListResponseDto>>> GetSubMenus([FromQuery] Guid? menuId = null, [FromQuery] bool includeInactive = false)
    {
        var query = new GetSubMenuListQuery { MenuId = menuId, IncludeInactive = includeInactive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateSubMenu([FromBody] CreateSubMenuCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(CreateSubMenu), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update submenu
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateSubMenu(Guid id, [FromBody] UpdateSubMenuCommand command)
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
    /// Delete submenu
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteSubMenu(Guid id)
    {
        var command = new DeleteSubMenuCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
