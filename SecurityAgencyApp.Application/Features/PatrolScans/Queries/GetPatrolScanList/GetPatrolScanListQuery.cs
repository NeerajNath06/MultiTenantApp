using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.PatrolScans.Queries.GetPatrolScanList;

public class GetPatrolScanListQuery : IRequest<ApiResponse<PatrolScanListResponseDto>>
{
    public Guid GuardId { get; set; }
    public Guid? SiteId { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public class PatrolScanListResponseDto
{
    public List<PatrolScanDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class PatrolScanDto
{
    public Guid Id { get; set; }
    public Guid GuardId { get; set; }
    public Guid SiteId { get; set; }
    public string? SiteName { get; set; }
    public DateTime ScannedAt { get; set; }
    public string LocationName { get; set; } = string.Empty;
    public string? CheckpointCode { get; set; }
    public string Status { get; set; } = string.Empty;
}
