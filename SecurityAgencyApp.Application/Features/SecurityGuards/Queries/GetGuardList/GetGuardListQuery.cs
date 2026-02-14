using MediatR;
using SecurityAgencyApp.Application.Common.Models;

namespace SecurityAgencyApp.Application.Features.SecurityGuards.Queries.GetGuardList;

public class GetGuardListQuery : IRequest<ApiResponse<GuardListResponseDto>>
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public bool IncludeInactive { get; set; } = false;
    public string? Search { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; } = "asc";
    /// <summary>Filter guards by assigned supervisor (User ID). When set, only guards under this supervisor are returned.</summary>
    public Guid? SupervisorId { get; set; }
}

public class GuardListResponseDto
{
    public List<GuardDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public string? SortBy { get; set; }
    public string? SortDirection { get; set; }
    public string? Search { get; set; }
}

public class GuardDto
{
    public Guid Id { get; set; }
    public string GuardCode { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string Gender { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedDate { get; set; }
    public Guid? SupervisorId { get; set; }
    public string? SupervisorName { get; set; }
}
