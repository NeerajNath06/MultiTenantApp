using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Shifts.Commands.UpdateShift;

public class UpdateShiftCommand : IRequest<ApiResponse<bool>>
{
    public Guid Id { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int BreakDuration { get; set; }
    public bool IsActive { get; set; }
}
