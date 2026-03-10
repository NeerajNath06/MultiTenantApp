using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SiteRates.Commands.DeleteSiteRatePlan;

public class DeleteSiteRatePlanCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
