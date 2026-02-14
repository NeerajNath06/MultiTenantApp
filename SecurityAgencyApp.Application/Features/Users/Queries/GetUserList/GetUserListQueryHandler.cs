using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Users.Queries.GetUserList;

public class GetUserListQueryHandler : IRequestHandler<GetUserListQuery, ApiResponse<UserListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetUserListQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<UserListResponseDto>> Handle(GetUserListQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<UserListResponseDto>.ErrorResponse("Tenant context not found");
        }

        var userRepo = _unitOfWork.Repository<User>();
        IList<User> users;
        if (!string.IsNullOrWhiteSpace(request.RoleCode))
        {
            var roleCode = request.RoleCode.Trim();
            // Load all roles for tenant, then filter in-memory (avoids EF translation issues with string.Contains/IndexOf)
            var allTenantRoles = (await _unitOfWork.Repository<Role>().FindAsync(
                r => r.TenantId == _tenantContext.TenantId.Value,
                cancellationToken)).ToList();
            var supervisorRoles = allTenantRoles.Where(r =>
                string.Equals(r.Code, roleCode, StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(roleCode, "SUPERVISOR", StringComparison.OrdinalIgnoreCase) &&
                 ((r.Name != null && r.Name.IndexOf("Supervisor", StringComparison.OrdinalIgnoreCase) >= 0) ||
                  (r.Code != null && r.Code.IndexOf("SUPERVISOR", StringComparison.OrdinalIgnoreCase) >= 0)))).ToList();
            if (supervisorRoles.Count == 0)
            {
                users = new List<User>();
            }
            else
            {
                var roleIds = supervisorRoles.Select(r => r.Id).ToList();
                var userIdsWithRole = await _unitOfWork.Repository<UserRole>().FindAsync(
                    ur => roleIds.Contains(ur.RoleId), cancellationToken);
                var userIds = userIdsWithRole.Select(ur => ur.UserId).Distinct().ToList();
                users = userIds.Any()
                    ? (await userRepo.FindAsync(
                        u => u.TenantId == _tenantContext.TenantId.Value &&
                             userIds.Contains(u.Id) &&
                             (string.IsNullOrEmpty(request.Search) ||
                              (u.FirstName != null && u.FirstName.Contains(request.Search)) ||
                              (u.LastName != null && u.LastName.Contains(request.Search)) ||
                              (u.Email != null && u.Email.Contains(request.Search)) ||
                              (u.UserName != null && u.UserName.Contains(request.Search))) &&
                             (!request.DepartmentId.HasValue || u.DepartmentId == request.DepartmentId) &&
                             (!request.DesignationId.HasValue || u.DesignationId == request.DesignationId) &&
                             (!request.IsActive.HasValue || u.IsActive == request.IsActive.Value),
                        cancellationToken)).ToList()
                    : new List<User>();
            }
        }
        else
        {
            users = (await userRepo.FindAsync(
                u => u.TenantId == _tenantContext.TenantId.Value &&
                     (string.IsNullOrEmpty(request.Search) ||
                      (u.FirstName != null && u.FirstName.Contains(request.Search)) ||
                      (u.LastName != null && u.LastName.Contains(request.Search)) ||
                      (u.Email != null && u.Email.Contains(request.Search)) ||
                      (u.UserName != null && u.UserName.Contains(request.Search))) &&
                     (!request.DepartmentId.HasValue || u.DepartmentId == request.DepartmentId) &&
                     (!request.DesignationId.HasValue || u.DesignationId == request.DesignationId) &&
                     (!request.IsActive.HasValue || u.IsActive == request.IsActive.Value),
                cancellationToken)).ToList();
        }

        var totalCount = users.Count();
        var pagedUsers = users
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        // Get departments and designations for mapping
        var departmentIds = pagedUsers.Where(u => u.DepartmentId.HasValue).Select(u => u.DepartmentId!.Value).Distinct().ToList();
        var departments = departmentIds.Any() 
            ? await _unitOfWork.Repository<Department>().FindAsync(d => departmentIds.Contains(d.Id), cancellationToken)
            : new List<Department>();

        var designationIds = pagedUsers.Where(u => u.DesignationId.HasValue).Select(u => u.DesignationId!.Value).Distinct().ToList();
        var designations = designationIds.Any()
            ? await _unitOfWork.Repository<Designation>().FindAsync(d => designationIds.Contains(d.Id), cancellationToken)
            : new List<Designation>();

        var userDtos = pagedUsers.Select(u => new UserDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            FirstName = u.FirstName,
            LastName = u.LastName,
            PhoneNumber = u.PhoneNumber,
            DepartmentId = u.DepartmentId,
            DepartmentName = departments.FirstOrDefault(d => d.Id == u.DepartmentId)?.Name,
            DesignationId = u.DesignationId,
            DesignationName = designations.FirstOrDefault(d => d.Id == u.DesignationId)?.Name,
            IsActive = u.IsActive,
            LastLoginDate = u.LastLoginDate
        }).ToList();

        var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

        var response = new UserListResponseDto
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            TotalPages = totalPages
        };

        return ApiResponse<UserListResponseDto>.SuccessResponse(response, "Users retrieved successfully");
    }
}
