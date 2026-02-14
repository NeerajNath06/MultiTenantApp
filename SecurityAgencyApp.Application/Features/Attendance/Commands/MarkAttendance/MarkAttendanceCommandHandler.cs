using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Attendance.Commands.MarkAttendance;

public class MarkAttendanceCommandHandler : IRequestHandler<MarkAttendanceCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public MarkAttendanceCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
        _currentUserService = currentUserService;
    }

    public async Task<ApiResponse<Guid>> Handle(MarkAttendanceCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Check if attendance already exists
        var existing = await _unitOfWork.Repository<GuardAttendance>().FirstOrDefaultAsync(
            a => a.GuardId == request.GuardId &&
                 a.AssignmentId == request.AssignmentId &&
                 a.AttendanceDate.Date == request.AttendanceDate.Date &&
                 a.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        GuardAttendance attendance;
        if (existing != null)
        {
            // Update existing
            existing.CheckInTime = request.CheckInTime ?? existing.CheckInTime;
            existing.CheckOutTime = request.CheckOutTime ?? existing.CheckOutTime;
            existing.CheckInLocation = request.CheckInLocation ?? existing.CheckInLocation;
            existing.CheckOutLocation = request.CheckOutLocation ?? existing.CheckOutLocation;
            existing.Status = request.Status;
            existing.Remarks = request.Remarks;
            existing.MarkedBy = _currentUserService.UserId;
            existing.ModifiedDate = DateTime.UtcNow;

            await _unitOfWork.Repository<GuardAttendance>().UpdateAsync(existing, cancellationToken);
            attendance = existing;
        }
        else
        {
            // Create new
            attendance = new GuardAttendance
            {
                TenantId = _tenantContext.TenantId.Value,
                GuardId = request.GuardId,
                AssignmentId = request.AssignmentId,
                AttendanceDate = request.AttendanceDate,
                CheckInTime = request.CheckInTime,
                CheckOutTime = request.CheckOutTime,
                CheckInLocation = request.CheckInLocation,
                CheckOutLocation = request.CheckOutLocation,
                Status = request.Status,
                Remarks = request.Remarks,
                MarkedBy = _currentUserService.UserId
            };

            await _unitOfWork.Repository<GuardAttendance>().AddAsync(attendance, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(attendance.Id, "Attendance marked successfully");
    }
}
