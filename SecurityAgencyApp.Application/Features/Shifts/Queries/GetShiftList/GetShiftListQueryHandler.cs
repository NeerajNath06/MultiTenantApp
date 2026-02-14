using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Shifts.Queries.GetShiftList;

public class GetShiftListQueryHandler : IRequestHandler<GetShiftListQuery, ApiResponse<ShiftListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetShiftListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<ShiftListResponseDto>> Handle(GetShiftListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<ShiftListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var shiftRepo = _unitOfWork.Repository<Shift>();
        var query = shiftRepo.GetQueryable()
            .Where(s => s.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || s.IsActive));

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(s => s.ShiftName.ToLower().Contains(search));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.ShiftName)
                : query.OrderBy(s => s.ShiftName),
            "start" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.StartTime)
                : query.OrderBy(s => s.StartTime),
            "end" => request.SortDirection == "desc"
                ? query.OrderByDescending(s => s.EndTime)
                : query.OrderBy(s => s.EndTime),
            _ => query.OrderBy(s => s.StartTime)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var shifts = await shiftRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var items = shifts.Select(s => new ShiftDto
        {
            Id = s.Id,
            ShiftName = s.ShiftName,
            StartTime = s.StartTime,
            EndTime = s.EndTime,
            BreakDuration = s.BreakDuration,
            IsActive = s.IsActive
        }).ToList();

        var response = new ShiftListResponseDto 
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
        return ApiResponse<ShiftListResponseDto>.SuccessResponse(response);
    }
}
