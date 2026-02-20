using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.GuardAssignments.Queries.GetAssignmentList;

public class GetAssignmentListQueryHandler : IRequestHandler<GetAssignmentListQuery, ApiResponse<AssignmentListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetAssignmentListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<AssignmentListResponseDto>> Handle(GetAssignmentListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<AssignmentListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var assignmentRepo = _unitOfWork.Repository<GuardAssignment>();
        var query = assignmentRepo.GetQueryable()
            .Where(a => a.TenantId == _tenantContext.TenantId.Value &&
                       (request.GuardId == null || a.GuardId == request.GuardId.Value) &&
                       (request.SiteId == null || a.SiteId == request.SiteId.Value) &&
                       (request.SupervisorId == null || a.SupervisorId == request.SupervisorId.Value) &&
                       (request.IncludeInactive || a.Status == Domain.Enums.AssignmentStatus.Active) &&
                       (request.DateTo == null || a.AssignmentStartDate <= request.DateTo.Value) &&
                       (request.DateFrom == null || (a.AssignmentEndDate == null || a.AssignmentEndDate >= request.DateFrom.Value)));

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "date" => request.SortDirection == "desc"
                ? query.OrderByDescending(a => a.AssignmentStartDate)
                : query.OrderBy(a => a.AssignmentStartDate),
            "status" => request.SortDirection == "desc"
                ? query.OrderByDescending(a => a.Status)
                : query.OrderBy(a => a.Status),
            _ => query.OrderByDescending(a => a.AssignmentStartDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var assignments = await assignmentRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var guards = await _unitOfWork.Repository<SecurityGuard>().FindAsync(
            g => assignments.Select(a => a.GuardId).Contains(g.Id), cancellationToken);
        var sites = await _unitOfWork.Repository<Site>().FindAsync(
            s => assignments.Select(a => a.SiteId).Contains(s.Id), cancellationToken);
        var shifts = await _unitOfWork.Repository<Shift>().FindAsync(
            sh => assignments.Select(a => a.ShiftId).Contains(sh.Id), cancellationToken);
        var supervisorIds = assignments.Where(a => a.SupervisorId.HasValue).Select(a => a.SupervisorId!.Value).Distinct().ToList();
        var supervisors = supervisorIds.Any()
            ? await _unitOfWork.Repository<User>().FindAsync(u => supervisorIds.Contains(u.Id), cancellationToken)
            : new List<User>();

        var items = assignments.Select(a =>
        {
            var guard = guards.FirstOrDefault(g => g.Id == a.GuardId);
            var supervisor = a.SupervisorId.HasValue ? supervisors.FirstOrDefault(s => s.Id == a.SupervisorId.Value) : null;
            var shift = shifts.FirstOrDefault(sh => sh.Id == a.ShiftId);
            return new AssignmentDto
            {
                Id = a.Id,
                GuardId = a.GuardId,
                GuardName = guard != null ? $"{guard.FirstName} {guard.LastName}".Trim() : "",
                GuardCode = guard?.GuardCode ?? "",
                SiteId = a.SiteId,
                SiteName = sites.FirstOrDefault(s => s.Id == a.SiteId)?.SiteName ?? "",
                ShiftId = a.ShiftId,
                ShiftName = shift?.ShiftName ?? "",
                ShiftStartTime = shift != null ? shift.StartTime.ToString(@"hh\:mm") : null,
                ShiftEndTime = shift != null ? shift.EndTime.ToString(@"hh\:mm") : null,
                AssignmentStartDate = a.AssignmentStartDate,
                AssignmentEndDate = a.AssignmentEndDate,
                Status = a.Status.ToString(),
                Remarks = a.Remarks,
                SupervisorId = a.SupervisorId,
                SupervisorName = supervisor != null ? $"{supervisor.FirstName} {supervisor.LastName}".Trim() : null
            };
        }).ToList();

        var response = new AssignmentListResponseDto 
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
        return ApiResponse<AssignmentListResponseDto>.SuccessResponse(response);
    }
}
