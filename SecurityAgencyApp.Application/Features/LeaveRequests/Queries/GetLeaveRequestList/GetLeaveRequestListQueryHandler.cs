using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.LeaveRequests.Queries.GetLeaveRequestList;

public class GetLeaveRequestListQueryHandler : IRequestHandler<GetLeaveRequestListQuery, ApiResponse<LeaveRequestListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetLeaveRequestListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<LeaveRequestListResponseDto>> Handle(GetLeaveRequestListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<LeaveRequestListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var leaveRepo = _unitOfWork.Repository<LeaveRequest>();
        var query = leaveRepo.GetQueryable()
            .Where(lr => lr.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || lr.IsActive));

        if (request.GuardId.HasValue)
        {
            query = query.Where(lr => lr.GuardId == request.GuardId.Value);
        }

        if (request.SupervisorId.HasValue)
        {
            var guardIdsUnderSupervisor = (await _unitOfWork.Repository<SecurityGuard>()
                .FindAsync(g => g.TenantId == _tenantContext.TenantId.Value && g.SupervisorId == request.SupervisorId.Value, cancellationToken))
                .Select(g => g.Id)
                .ToList();
            query = query.Where(lr => guardIdsUnderSupervisor.Contains(lr.GuardId));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(lr => lr.Status == request.Status);
        }

        if (!string.IsNullOrWhiteSpace(request.LeaveType))
        {
            query = query.Where(lr => lr.LeaveType == request.LeaveType);
        }

        query = request.SortBy?.ToLower() switch
        {
            "startdate" => request.SortDirection == "asc"
                ? query.OrderBy(lr => lr.StartDate)
                : query.OrderByDescending(lr => lr.StartDate),
            "status" => request.SortDirection == "asc"
                ? query.OrderBy(lr => lr.Status)
                : query.OrderByDescending(lr => lr.Status),
            _ => query.OrderByDescending(lr => lr.StartDate)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var leaveRequests = await leaveRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        // Get guard names
        var guardIds = leaveRequests.Select(lr => lr.GuardId).Distinct().ToList();
        var guards = guardIds.Any()
            ? await _unitOfWork.Repository<SecurityGuard>().FindAsync(g => guardIds.Contains(g.Id), cancellationToken)
            : new List<SecurityGuard>();

        var leaveDtos = leaveRequests.Select(lr => new LeaveRequestDto
        {
            Id = lr.Id,
            GuardId = lr.GuardId,
            GuardName = $"{guards.FirstOrDefault(g => g.Id == lr.GuardId)?.FirstName} {guards.FirstOrDefault(g => g.Id == lr.GuardId)?.LastName}",
            GuardCode = guards.FirstOrDefault(g => g.Id == lr.GuardId)?.GuardCode ?? "",
            LeaveType = lr.LeaveType,
            StartDate = lr.StartDate,
            EndDate = lr.EndDate,
            TotalDays = lr.TotalDays,
            Reason = lr.Reason,
            Status = lr.Status,
            ApprovedDate = lr.ApprovedDate,
            IsActive = lr.IsActive,
            CreatedDate = lr.CreatedDate
        }).ToList();

        var response = new LeaveRequestListResponseDto
        {
            Items = leaveDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<LeaveRequestListResponseDto>.SuccessResponse(response, "Leave requests retrieved successfully");
    }
}
