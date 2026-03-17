using MediatR;
using Microsoft.AspNetCore.Mvc;
using SecurityAgencyApp.Application.Features.SitePosts;
using SecurityAgencyApp.Application.Features.SitePosts.Commands.CreateSitePost;
using SecurityAgencyApp.Application.Features.SitePosts.Commands.DeleteSitePost;
using SecurityAgencyApp.Application.Features.SitePosts.Commands.UpdateSitePost;
using SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostById;
using SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostList;

namespace SecurityAgencyApp.API.Controllers.v1;

[Route("api/v1/[controller]")]
public class SitePostsController : GenericCrudControllerBase<
    SitePostListResponseDto,
    SitePostDto,
    GetSitePostListQuery,
    GetSitePostByIdQuery,
    CreateSitePostCommand,
    UpdateSitePostCommand,
    DeleteSitePostCommand>
{
    public SitePostsController(IMediator mediator) : base(mediator) { }

    protected override GetSitePostByIdQuery CreateGetByIdQuery(Guid id) => new() { Id = id };

    protected override void SetUpdateCommandId(UpdateSitePostCommand command, Guid id) => command.Id = id;

    protected override DeleteSitePostCommand CreateDeleteCommand(Guid id) => new() { Id = id };
}
