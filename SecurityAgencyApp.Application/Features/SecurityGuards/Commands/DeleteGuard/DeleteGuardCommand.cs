using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Commands.DeleteGuard;

public class DeleteGuardCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
