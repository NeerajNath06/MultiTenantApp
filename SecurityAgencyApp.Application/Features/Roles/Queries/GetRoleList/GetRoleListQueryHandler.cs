using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Queries.GetRoleList;

public class GetRoleListQueryHandler : IRequestHandler<GetRoleListQuery, ApiResponse<RoleListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetRoleListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<RoleListResponseDto>> Handle(GetRoleListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<RoleListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var roleRepo = _unitOfWork.Repository<Role>();
        var query = roleRepo.GetQueryable()
            .Where(r => r.TenantId == _tenantContext.TenantId.Value &&
                       (request.IncludeInactive || r.IsActive));

        // Apply search filter
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.ToLower();
            query = query.Where(r =>
                r.Name.ToLower().Contains(search) ||
                r.Code.ToLower().Contains(search) ||
                (r.Description != null && r.Description.ToLower().Contains(search)));
        }

        // Apply sorting
        query = request.SortBy?.ToLower() switch
        {
            "name" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.Name)
                : query.OrderBy(r => r.Name),
            "code" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.Code)
                : query.OrderBy(r => r.Code),
            "created" => request.SortDirection == "desc"
                ? query.OrderByDescending(r => r.CreatedDate)
                : query.OrderBy(r => r.CreatedDate),
            _ => query.OrderBy(r => r.Name)
        };

        var totalCount = query.Count();
        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);
        var roles = await roleRepo.GetPagedAsync(query, request.PageNumber, request.PageSize, cancellationToken);

        var roleDtos = new List<RoleDto>();

        foreach (var role in roles)
        {
            // Get user count
            var userCount = await _unitOfWork.Repository<UserRole>().CountAsync(
                ur => ur.RoleId == role.Id,
                cancellationToken);

            // Get permission count
            var permissionCount = await _unitOfWork.Repository<RolePermission>().CountAsync(
                rp => rp.RoleId == role.Id,
                cancellationToken);

            roleDtos.Add(new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Code = role.Code,
                Description = role.Description,
                IsSystemRole = role.IsSystemRole,
                IsActive = role.IsActive,
                UserCount = userCount,
                PermissionCount = permissionCount,
                CreatedDate = role.CreatedDate
            });
        }

        var response = new RoleListResponseDto
        {
            Items = roleDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Search = request.Search
        };

        return ApiResponse<RoleListResponseDto>.SuccessResponse(response, "Roles retrieved successfully");
    }
}
