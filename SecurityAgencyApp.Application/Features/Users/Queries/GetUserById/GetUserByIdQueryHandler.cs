using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Users.Queries.GetUserById;

public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, ApiResponse<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public GetUserByIdQueryHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<UserDto>> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<UserDto>.ErrorResponse("Tenant context not found");
        }

        var userRepo = _unitOfWork.Repository<User>();
        var user = await userRepo.GetByIdAsync(request.Id, cancellationToken);

        if (user == null || user.TenantId != _tenantContext.TenantId.Value)
        {
            return ApiResponse<UserDto>.ErrorResponse("User not found");
        }

        // Get department
        Department? department = null;
        if (user.DepartmentId.HasValue)
        {
            department = await _unitOfWork.Repository<Department>().GetByIdAsync(user.DepartmentId.Value, cancellationToken);
        }

        // Get designation
        Designation? designation = null;
        if (user.DesignationId.HasValue)
        {
            designation = await _unitOfWork.Repository<Designation>().GetByIdAsync(user.DesignationId.Value, cancellationToken);
        }

        // Get roles
        var userRoles = await _unitOfWork.Repository<UserRole>().FindAsync(
            ur => ur.UserId == user.Id, cancellationToken);
        
        var roleIds = userRoles.Select(ur => ur.RoleId).ToList();
        var roles = roleIds.Any()
            ? await _unitOfWork.Repository<Role>().FindAsync(r => roleIds.Contains(r.Id), cancellationToken)
            : new List<Role>();

        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            PhoneNumber = user.PhoneNumber,
            AadharNumber = user.AadharNumber,
            PANNumber = user.PANNumber,
            UAN = user.UAN,
            DepartmentId = user.DepartmentId,
            DepartmentName = department?.Name,
            DesignationId = user.DesignationId,
            DesignationName = designation?.Name,
            IsActive = user.IsActive,
            Roles = roles.Select(r => r.Name).ToList(),
            RoleIds = roleIds,
            CreatedDate = user.CreatedDate
        };

        return ApiResponse<UserDto>.SuccessResponse(userDto, "User retrieved successfully");
    }
}
