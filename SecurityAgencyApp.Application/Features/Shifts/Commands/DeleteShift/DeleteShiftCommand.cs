using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.DeleteShift;

public class DeleteShiftCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
}
