using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftById;

public class GetShiftByIdQuery : IRequest<ApiResponse<ShiftDto>>
{
    public Guid Id { get; set; }
}

public class ShiftDto
{
    public Guid Id { get; set; }
    public string ShiftName { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public int BreakDuration { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
}
