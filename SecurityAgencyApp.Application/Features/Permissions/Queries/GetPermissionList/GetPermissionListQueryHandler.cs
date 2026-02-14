using MediatR;
using SecurityAgencyApp.Application.Common.Models;
using SecurityAgencyApp.Application.Interfaces;
using SecurityAgencyApp.Domain.Entities;

namespace SecurityAgencyApp.Application.Features.Permissions.Queries.GetPermissionList;

public class GetPermissionListQueryHandler : IRequestHandler<GetPermissionListQuery, ApiResponse<PermissionListResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPermissionListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiResponse<PermissionListResponseDto>> Handle(GetPermissionListQuery request, CancellationToken cancellationToken)
    {
        var permissionRepo = _unitOfWork.Repository<Permission>();
        
        IEnumerable<Permission> permissions;
        if (request.IncludeInactive)
        {
            permissions = await permissionRepo.GetAllAsync(cancellationToken);
        }
        else
        {
            permissions = await permissionRepo.FindAsync(p => p.IsActive, cancellationToken);
        }

        var items = permissions
            .OrderBy(p => p.Resource)
            .ThenBy(p => p.Action)
            .Select(p => new PermissionDto
            {
                Id = p.Id,
                Resource = p.Resource,
                Action = p.Action,
                Description = p.Description,
                IsActive = p.IsActive
            })
            .ToList();

        var response = new PermissionListResponseDto
        {
            Items = items,
            TotalCount = items.Count
        };

        return ApiResponse<PermissionListResponseDto>.SuccessResponse(response);
    }
}
