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

[Route("api/v1/[controller]")]
public class SitesController : GenericCrudControllerBase<
    SiteListResponseDto,
    GetSiteById.SiteDto,
    GetSiteListQuery,
    GetSiteByIdQuery,
    CreateSiteCommand,
    UpdateSiteCommand,
    DeleteSiteCommand>
{
    private readonly IMediator _mediator;

    public SitesController(IMediator mediator) : base(mediator)
    {
        _mediator = mediator;
    }

    protected override GetSiteByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateSiteCommand command, Guid id) => command.Id = id;

    protected override DeleteSiteCommand CreateDeleteCommand(Guid id) => new() { Id = id };

    [HttpGet("{id}/Supervisors")]
    public async Task<ActionResult<ApiResponse<GetSupervisorsBySiteResponse>>> GetSupervisorsBySite(Guid id)
    {
        var query = new GetSupervisorsBySiteQuery { SiteId = id };
        var result = await _mediator.Send(query);
        if (!result.Success)
            return BadRequest(result);
        return Ok(result);
    }
}
