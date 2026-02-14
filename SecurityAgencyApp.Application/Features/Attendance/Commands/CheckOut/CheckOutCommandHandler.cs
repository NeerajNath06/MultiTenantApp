using MediatR;
using SecurityAgencyApp.Application.Common;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Attendance.Commands.CheckOut;

public class CheckOutCommandHandler : IRequestHandler<CheckOutCommand, ApiResponse<CheckOutResultDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CheckOutCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<CheckOutResultDto>> Handle(CheckOutCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<CheckOutResultDto>.ErrorResponse("Tenant context not found");

        var attendance = await _unitOfWork.Repository<GuardAttendance>().GetByIdAsync(request.AttendanceId, cancellationToken);
        if (attendance == null || attendance.GuardId != request.GuardId || attendance.TenantId != _tenantContext.TenantId.Value || attendance.CheckOutTime != null)
            return ApiResponse<CheckOutResultDto>.ErrorResponse("Attendance record not found or already checked out.");

        var assignment = await _unitOfWork.Repository<GuardAssignment>().GetByIdAsync(attendance.AssignmentId, cancellationToken);
        if (assignment == null)
            return ApiResponse<CheckOutResultDto>.ErrorResponse("Assignment not found.");

        var site = await _unitOfWork.Repository<Site>().GetByIdAsync(assignment.SiteId, cancellationToken);
        if (site == null)
            return ApiResponse<CheckOutResultDto>.ErrorResponse("Site not found.");
        if (site.Latitude == null || site.Longitude == null)
            return ApiResponse<CheckOutResultDto>.ErrorResponse("Site location is not configured. Contact your supervisor.");

        var radiusMeters = site.GeofenceRadiusMeters ?? 100;
        var distanceMeters = GeoHelper.GetDistanceMeters(
            site.Latitude.Value, site.Longitude.Value,
            request.Latitude, request.Longitude);

        if (distanceMeters > radiusMeters)
        {
            var distanceText = distanceMeters < 1000
                ? $"{Math.Round(distanceMeters, 0)} m"
                : $"{Math.Round(distanceMeters / 1000, 2)} km";
            return ApiResponse<CheckOutResultDto>.ErrorResponse(
                $"Check-out is only allowed at the allocated site. You are {distanceText} away from the site (allowed: {radiusMeters} m). Please reach the site to check out.");
        }

        var locationStr = $"{request.Latitude:F6},{request.Longitude:F6}";
        // Store check-out time in app timezone (IST) so DB shows local time
        var nowLocal = AppTimeHelper.NowInAppTimeZone();
        attendance.CheckOutTime = request.CheckOutTime.HasValue
            ? (request.CheckOutTime.Value.Kind == DateTimeKind.Utc
                ? AppTimeHelper.UtcToAppTimeZone(request.CheckOutTime.Value)
                : request.CheckOutTime.Value)
            : nowLocal;
        attendance.CheckOutLocation = locationStr;
        if (!string.IsNullOrWhiteSpace(request.Notes))
            attendance.Remarks = (attendance.Remarks ?? "") + " [Check-out: " + request.Notes + "]";

        await _unitOfWork.Repository<GuardAttendance>().UpdateAsync(attendance, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<CheckOutResultDto>.SuccessResponse(
            new CheckOutResultDto { Message = "Check-out successful." },
            "Check-out successful.");
    }
}
