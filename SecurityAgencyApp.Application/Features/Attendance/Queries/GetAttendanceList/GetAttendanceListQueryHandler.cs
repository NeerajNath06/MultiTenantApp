using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Attendance.Queries.GetAttendanceList;

public class GetAttendanceListQueryHandler : IRequestHandler<GetAttendanceListQuery, ApiResponse<AttendanceListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetAttendanceListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<AttendanceListResponseDto>> Handle(GetAttendanceListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<AttendanceListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var attendanceRepo = _unitOfWork.Repository<GuardAttendance>();
        var query = attendanceRepo.GetQueryable()
            .Where(a => a.TenantId == _tenantContext.TenantId.Value);

        if (request.GuardId.HasValue)
            query = query.Where(a => a.GuardId == request.GuardId.Value);
        if (request.AssignmentId.HasValue)
            query = query.Where(a => a.AssignmentId == request.AssignmentId.Value);
        if (request.StartDate.HasValue)
            query = query.Where(a => a.AttendanceDate.Date >= request.StartDate.Value.Date);
        if (request.EndDate.HasValue)
            query = query.Where(a => a.AttendanceDate.Date <= request.EndDate.Value.Date);
        if (!string.IsNullOrEmpty(request.Status))
            query = query.Where(a => a.Status.ToString() == request.Status);

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            // Search will be applied after loading related data
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "date" => request.SortDirection == "desc"
                ? query.OrderByDescending(a => a.AttendanceDate)
                : query.OrderBy(a => a.AttendanceDate),
            "status" => request.SortDirection == "desc"
                ? query.OrderByDescending(a => a.Status)
                : query.OrderBy(a => a.Status),
            "checkin" => request.SortDirection == "desc"
                ? query.OrderByDescending(a => a.CheckInTime)
                : query.OrderBy(a => a.CheckInTime),
            _ => query.OrderByDescending(a => a.AttendanceDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var attendances = await attendanceRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var guardIds = attendances.Select(a => a.GuardId).Distinct().ToList();
        var assignmentIds = attendances.Select(a => a.AssignmentId).Distinct().ToList();

        var guards = guardIds.Any()
            ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)
            : new List<SecurityGuard>();
        var assignments = assignmentIds.Any()
            ? await _unitOfWork.Repository<GuardAssignment>().FindAsync(a => assignmentIds.Contains(a.Id), cancellationToken)
            : new List<GuardAssignment>();
        var sites = assignments.Any()
            ? await _unitOfWork.Repository<Site>().FindAsync(s => assignments.Select(a => a.SiteId).Contains(s.Id), cancellationToken)
            : new List<Site>();

        var items = attendances.Select(a =>
        {
            var g = guards.FirstOrDefault(g => g.Id == a.GuardId);
            var ass = assignments.FirstOrDefault(ass => ass.Id == a.AssignmentId);
            var site = sites.FirstOrDefault(s => s.Id == ass?.SiteId);
            return new AttendanceDto
            {
                Id = a.Id,
                GuardId = a.GuardId,
                GuardName = $"{g?.FirstName} {g?.LastName}".Trim(),
                GuardCode = g?.GuardCode ?? "",
                GuardPhone = g?.PhoneNumber,
                AssignmentId = a.AssignmentId,
                SiteName = site?.SiteName ?? "",
                AttendanceDate = a.AttendanceDate,
                CheckInTime = a.CheckInTime,
                CheckOutTime = a.CheckOutTime,
                Status = a.Status.ToString(),
                Remarks = a.Remarks
            };
        }).ToList();

        // Apply search filter on results if needed
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            items = items.Where(a => 
                a.GuardName.ToLower().Contains(search) ||
                a.GuardCode.ToLower().Contains(search) ||
                a.SiteName.ToLower().Contains(search)).ToList();
            totalCount = items.Count;
            totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        }

        var response = new AttendanceListResponseDto 
        { 
            Items = items,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };
        return ApiResponse<AttendanceListResponseDto>.SuccessResponse(response);
    }
}
