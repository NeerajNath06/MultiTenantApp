using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.FormBuilder.Commands.CreateFormTemplate;
using SecurityAgencyApp.Application.Features.FormBuilder.Commands.SubmitForm;
using SecurityAgencyApp.Application.Features.FormBuilder.Queries.GetFormTemplateById;
using SecurityAgencyApp.Application.Features.FormBuilder.Queries.GetFormTemplateList;

namespace SecurityAgencyApp.API.Controllers.v1;

[ApiController]
[Route("api/v1/[controller]")]
public class FormBuilderController : ControllerBase
{
    private readonly IMediator _mediator;

    public FormBuilderController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("templates")]
    public async Task<ActionResult<ApiResponse<FormTemplateListResponseDto>>> GetFormTemplates([FromQuery] bool includeInactive = false, [FromQuery] string? category = null)
    {
        var query = new GetFormTemplateListQuery { IncludeInactive = includeInactive, Category = category };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("templates/{id}")]
    public async Task<ActionResult<ApiResponse<Application.Features.FormBuilder.Queries.GetFormTemplateList.FormTemplateDto>>> GetFormTemplate(Guid id)
    {
        var query = new GetFormTemplateByIdQuery { Id = id };
        var result = await _mediator.Send(query);
        if (result.Success)
        {
            return Ok(result);
        }
        return NotFound(result);
    }

    /// <summary>
    /// Create form template
    /// </summary>
    [HttpPost("templates")]
    public async Task<ActionResult<ApiResponse<Guid>>> CreateFormTemplate([FromBody] CreateFormTemplateCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return CreatedAtAction(nameof(GetFormTemplate), new { id = result.Data }, result);
        }
        return BadRequest(result);
    }

    /// <summary>
    /// Submit form
    /// </summary>
    [HttpPost("submit")]
    public async Task<ActionResult<ApiResponse<Guid>>> SubmitForm([FromBody] SubmitFormCommand command)
    {
        var result = await _mediator.Send(command);
        if (result.Success)
        {
            return Ok(result);
        }
        return BadRequest(result);
    }
}
