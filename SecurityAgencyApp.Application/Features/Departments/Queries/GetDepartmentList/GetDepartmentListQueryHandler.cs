using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Departments.Queries.GetDepartmentList;

public class GetDepartmentListQueryHandler : IRequestHandler<GetDepartmentListQuery, ApiResponse<DepartmentListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetDepartmentListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<DepartmentListResponseDto>> Handle(GetDepartmentListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<DepartmentListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var departmentRepo = _unitOfWork.Repository<Department>();
        var query = departmentRepo.GetQueryable()
            .Where(d => d.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || d.IsActive));

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(d =>
                d.Name.ToLower().Contains(search) ||
                d.Code.ToLower().Contains(search) ||
                (d.Description != null && d.Description.ToLower().Contains(search)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(d => d.Name)
                : query.OrderBy(d => d.Name),
            "code" => request.SortDirection == "desc"
                ? query.OrderByDescending(d => d.Code)
                : query.OrderBy(d => d.Code),
            "created" => request.SortDirection == "desc"
                ? query.OrderByDescending(d => d.CreatedDate)
                : query.OrderBy(d => d.CreatedDate),
            _ => query.OrderBy(d => d.Name)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var departments = await departmentRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var departmentDtos = new List<DepartmentDto>();

        foreach (var dept in departments)
        {
            // Get user count
            var userCount = await _unitOfWork.Repository<User>().CountAsync(
                u => u.DepartmentId == dept.Id && u.TenantId == _tenantContext.TenantId.Value,
                cancellationToken);

            departmentDtos.Add(new DepartmentDto
            {
                Id = dept.Id,
                Name = dept.Name,
                Code = dept.Code,
                Description = dept.Description,
                IsActive = dept.IsActive,
                UserCount = userCount,
                CreatedDate = dept.CreatedDate
            });
        }

        var response = new DepartmentListResponseDto
        {
            Items = departmentDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<DepartmentListResponseDto>.SuccessResponse(response, "Departments retrieved successfully");
    }
}
