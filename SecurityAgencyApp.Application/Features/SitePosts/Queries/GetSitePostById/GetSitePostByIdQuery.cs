using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.SitePosts;

namespace SecurityAgencyApp.Application.Features.SitePosts.Queries.GetSitePostById;

public class GetSitePostByIdQuery : IRequest<ApiResponse<SitePostDto>>
{
    public Guid Id { get; set; }
}
