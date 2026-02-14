using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Visitors.Commands.UpdateVisitorExit;

public class UpdateVisitorExitCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public DateTime ExitTime { get; set; }
}
