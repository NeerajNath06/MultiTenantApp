using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SitePosts.Commands.DeleteSitePost;

public class DeleteSitePostCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
