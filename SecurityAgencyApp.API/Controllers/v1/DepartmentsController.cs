using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Departments.Commands.CreateDepartment;
using SecurityAgencyApp.Application.Features.Departments.Commands.DeleteDepartment;
using SecurityAgencyApp.Application.Features.Departments.Commands.UpdateDepartment;
using SecurityAgencyApp.Application.Features.Departments.Queries.GetDepartmentList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class DepartmentsController : ControllerBase
{
    private readonly IMediator _mediator;

    public DepartmentsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all departments
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<DepartmentListResponseDto>>> GetDepartments([FromQuery] bool includeInactive = false)
    {
        var query = new GetDepartmentListQuery { IncludeInactive = includeInactive };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateDepartment([FromBody] CreateDepartmentCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetDepartments), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Update department
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> UpdateDepartment(Guid id, [FromBody] UpdateDepartmentCommand command)
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
    /// Delete department
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteDepartment(Guid id)
    {
        var command = new DeleteDepartmentCommand { Id = id };
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
