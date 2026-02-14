using MediatR;
using SecurityAgencyApp.Application.Common;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Attendance.Queries.GetGuardAttendanceByDate;

public class GetGuardAttendanceByDateQueryHandler : IRequestHandler<GetGuardAttendanceByDateQuery, ApiResponse<List<GuardAttendanceItemDto>>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetGuardAttendanceByDateQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<List<GuardAttendanceItemDto>>> Handle(GetGuardAttendanceByDateQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
            return ApiResponse<List<GuardAttendanceItemDto>>.ErrorResponse("Tenant context not found");

        var date = (request.Date ?? AppTimeHelper.NowInAppTimeZone()).Date;
        var attendanceRepo = _unitOfWork.Repository<GuardAttendance>();
        var attendances = await attendanceRepo.FindAsync(
            a => a.GuardId == request.GuardId &&
                 a.TenantId == _tenantContext.TenantId.Value &&
                 a.AttendanceDate == date,
            cancellationToken);

        if (attendances == null || !attendances.Any())
            return ApiResponse<List<GuardAttendanceItemDto>>.SuccessResponse(new List<GuardAttendanceItemDto>(), "No attendance records.");

        var assignmentIds = attendances.Select(a => a.AssignmentId).Distinct().ToList();
        var assignments = await _unitOfWork.Repository<GuardAssignment>().FindAsync(a => assignmentIds.Contains(a.Id), cancellationToken);
        var siteIds = assignments.Select(a => a.SiteId).Distinct().ToList();
        var sites = await _unitOfWork.Repository<Site>().FindAsync(s => siteIds.Contains(s.Id), cancellationToken);

        var items = attendances.Select(a =>
        {
            var assignment = assignments.FirstOrDefault(ass => ass.Id == a.AssignmentId);
            var site = assignment != null ? sites.FirstOrDefault(s => s.Id == assignment.SiteId) : null;
            return new GuardAttendanceItemDto
            {
                Id = a.Id,
                GuardId = a.GuardId,
                AssignmentId = a.AssignmentId,
                SiteName = site?.SiteName ?? "",
                SiteLatitude = site?.Latitude,
                SiteLongitude = site?.Longitude,
                SiteGeofenceRadiusMeters = site?.GeofenceRadiusMeters,
                AttendanceDate = a.AttendanceDate,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Remarks = a.Remarks
            };
        }).ToList();

        return ApiResponse<List<GuardAttendanceItemDto>>.SuccessResponse(items, "Attendance retrieved.");
    }
}
