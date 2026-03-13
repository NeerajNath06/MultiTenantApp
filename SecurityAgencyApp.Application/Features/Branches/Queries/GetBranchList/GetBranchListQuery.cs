using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Features.Branches;

namespace SecurityAgencyApp.Application.Features.Branches.Queries.GetBranchList;

public class GetBranchListQuery : IRequest<ApiResponse<BranchListResponseDto>>
{
    public bool IncludeInactive { get; set; }
    public string? Search { get; set; }
}

public class BranchListResponseDto
{
    public List<BranchDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}
