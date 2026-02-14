using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.Permissions.Queries.GetPermissionList;

public class GetPermissionListQuery : IRequest<ApiResponse<PermissionListResponseDto>>
{
    public bool IncludeInactive { get; set; } = false;
}

public class PermissionListResponseDto
{
    public List<PermissionDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class PermissionDto
{
    public Guid Id { get; set; }
    public string Resource { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
}
