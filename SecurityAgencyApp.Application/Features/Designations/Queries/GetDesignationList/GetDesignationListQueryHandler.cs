using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Designations.Queries.GetDesignationList;

public class GetDesignationListQueryHandler : IRequestHandler<GetDesignationListQuery, ApiResponse<DesignationListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetDesignationListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<DesignationListResponseDto>> Handle(GetDesignationListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<DesignationListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var designationRepo = _unitOfWork.Repository<Designation>();
        var query = designationRepo.GetQueryable()
            .Where(d => d.TenantId == _tenantContext.TenantId.Value &&
                       (request.DepartmentId == null || d.DepartmentId == request.DepartmentId.Value) &&
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
        var designations = await designationRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var departmentIds = designations.Where(d => d.DepartmentId.HasValue).Select(d => d.DepartmentId!.Value).Distinct().ToList();
        var departments = departmentIds.Any()
            ? await _unitOfWork.Repository<Department>().FindAsync(d => departmentIds.Contains(d.Id), cancellationToken)
            : new List<Department>();

        var designationDtos = designations.Select(d => new DesignationDto
        {
            Id = d.Id,
            Name = d.Name,
            Code = d.Code,
            DepartmentId = d.DepartmentId,
            DepartmentName = departments.FirstOrDefault(dept => dept.Id == d.DepartmentId)?.Name,
            Description = d.Description,
            IsActive = d.IsActive,
            CreatedDate = d.CreatedDate
        }).ToList();

        var response = new DesignationListResponseDto
        {
            Items = designationDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<DesignationListResponseDto>.SuccessResponse(response);
    }
}
