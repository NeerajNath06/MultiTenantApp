using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Sites.Commands.DeleteSite;

public class DeleteSiteCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
