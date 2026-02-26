using MediatR;
using SecurityAgencyApp.Application.Common;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;
using SecurityAgencyApp.Domain.Enums;

namespace SecurityAgencyApp.Application.Features.Attendance.Commands.CheckIn;

public class CheckInCommandHandler : IRequestHandler<CheckInCommand, ApiResponse<CheckInResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CheckInCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<CheckInResultDto>> Handle(CheckInCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<CheckInResultDto>.ErrorResponse("Tenant context not found");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(request.SiteId, cancellationToken);
        if (site == null || site.TenantId != _tenantContext.TenantId.Value)
            return ApiResponse<CheckInResultDto>.ErrorResponse("Site not found");

        if (site.Latitude == null || site.Longitude == null)
            return ApiResponse<CheckInResultDto>.ErrorResponse("Site location is not configured. Contact your supervisor.");

        var radiusMeters = site.GeofenceRadiusMeters ?? 100;
        var distanceMeters = GeoHelper.GetDistanceMeters(
            site.Latitude.Value, site.Longitude.Value,
            request.Latitude, request.Longitude);

        if (distanceMeters > radiusMeters)
        {
            var distanceText = distanceMeters < 1000
                ? $"{Math.Round(distanceMeters, 0)} m"
                : $"{Math.Round(distanceMeters / 1000, 2)} km";
            return ApiResponse<CheckInResultDto>.ErrorResponse(
                $"Check-in is only allowed at the allocated site. You are {distanceText} away from the site (allowed: {radiusMeters} m). Please reach the site to check in.");
        }

        var nowLocal = AppTimeHelper.NowInAppTimeZone();
        var today = AppTimeHelper.TodayInAppTimeZone();
        var assignments = await _unitOfWork.Repository<GuardAssignment>()
            .FindAsync(a =>
                a.GuardId == request.GuardId &&
                a.SiteId == request.SiteId &&
                a.ShiftId == request.ShiftId &&
                a.TenantId == _tenantContext.TenantId.Value &&
                a.Status == AssignmentStatus.Active &&
                a.AssignmentStartDate <= today &&
                (a.AssignmentEndDate == null || a.AssignmentEndDate >= today),
                cancellationToken);

        var assignment = assignments.FirstOrDefault();
        if (assignment == null)
            return ApiResponse<CheckInResultDto>.ErrorResponse("No active assignment found for this site and shift. Contact your supervisor.");

        var shift = await _unitOfWork.Repository<Shift>().GetByIdAsync(assignment.ShiftId, cancellationToken);
        if (shift == null)
            return ApiResponse<CheckInResultDto>.ErrorResponse("Shift not found.");
        var nowTime = nowLocal.TimeOfDay;
        bool withinShift;
        if (shift.EndTime > shift.StartTime)
            withinShift = nowTime >= shift.StartTime && nowTime <= shift.EndTime;
        else
            withinShift = nowTime >= shift.StartTime || nowTime < shift.EndTime; // overnight shift
        if (!withinShift)
            return ApiResponse<CheckInResultDto>.ErrorResponse(
                "Punch-in is only allowed during your shift time (" + shift.StartTime.ToString(@"hh\:mm") + " - " + shift.EndTime.ToString(@"hh\:mm") + "). Current time is outside this window. Please check in when your shift starts.");

        var existing = await _unitOfWork.Repository<GuardAttendance>().FirstOrDefaultAsync(
            a => a.GuardId == request.GuardId &&
                 a.AssignmentId == assignment.Id &&
                 a.AttendanceDate == today &&
                 a.TenantId == _tenantContext.TenantId.Value &&
                 a.CheckInTime != null,
            cancellationToken);

        if (existing != null)
            return ApiResponse<CheckInResultDto>.ErrorResponse("You have already checked in for this shift today.");

        var locationStr = $"{request.Latitude:F6},{request.Longitude:F6}";
        // Store check-in time in app timezone (IST) so DB shows local time e.g. 1:55 PM
        var checkInTime = request.CheckInTime.HasValue
            ? (request.CheckInTime.Value.Kind == DateTimeKind.Utc
                ? AppTimeHelper.UtcToAppTimeZone(request.CheckInTime.Value)
                : request.CheckInTime.Value)
            : nowLocal;
        var attendance = new GuardAttendance
        {
            TenantId = _tenantContext.TenantId.Value,
            GuardId = request.GuardId,
            AssignmentId = assignment.Id,
            AttendanceDate = today,
            CheckInTime = checkInTime,
            CheckInLocation = locationStr,
            Status = AttendanceStatus.Present,
            Remarks = request.Notes
        };

        await _unitOfWork.Repository<GuardAttendance>().AddAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CheckInResultDto>.SuccessResponse(
            new CheckInResultDto { AttendanceId = attendance.Id, Message = "Check-in successful." },
            "Check-in successful.");
    }
}
