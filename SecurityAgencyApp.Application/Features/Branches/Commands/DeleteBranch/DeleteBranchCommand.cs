using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Branches.Commands.DeleteBranch;

public class DeleteBranchCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
