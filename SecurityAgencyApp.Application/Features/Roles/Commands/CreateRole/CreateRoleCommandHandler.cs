using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Roles.Commands.CreateRole;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponse<Guid>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITenantContext _tenantContext;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork, ITenantContext tenantContext)
    {
        _unitOfWork = unitOfWork;
        _tenantContext = tenantContext;
    }

    public async Task<ApiResponse<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            return ApiResponse<Guid>.ErrorResponse("Tenant context not found");
        }

        // Check if code already exists
        var roleRepo = _unitOfWork.Repository<Role>();
        var existing = await roleRepo.FirstOrDefaultAsync(
            r => r.Code == request.Code && r.TenantId == _tenantContext.TenantId.Value,
            cancellationToken);

        if (existing != null)
        {
            return ApiResponse<Guid>.ErrorResponse("Role code already exists");
        }

        var role = new Role
        {
            TenantId = _tenantContext.TenantId.Value,
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            IsSystemRole = false,
            IsActive = true
        };

        await roleRepo.AddAsync(role, cancellationToken);

        // Assign permissions if provided
        if (request.PermissionIds != null && request.PermissionIds.Any())
        {
            foreach (var permissionId in request.PermissionIds)
            {
                var rolePermission = new RolePermission
                {
                    RoleId = role.Id,
                    PermissionId = permissionId
                };
                await _unitOfWork.Repository<RolePermission>().AddAsync(rolePermission, cancellationToken);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return ApiResponse<Guid>.SuccessResponse(role.Id, "Role created successfully");
    }
}
