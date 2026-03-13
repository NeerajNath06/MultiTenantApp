using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Branches;

namespace SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchById;

public class GetBranchByIdQuery : IRequest<ApiResponse<BranchDto>>
{
    public Guid Id { get; set; }
}
