using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.CreateShift;

public class CreateShiftCommand : IRequest<ApiResponse<Guid>>
{
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int BreakDuration { get; set; } = 0;
}
